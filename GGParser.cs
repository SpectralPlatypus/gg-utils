using System.Text;

namespace GGUtils
{
    class GGParser
    {
        BinaryReader reader;
        List<int> nameList = new();

        public GGParser(BinaryReader reader)
        {
            this.reader = reader;
        }

        public void ParseHeader()
        {
            uint csum = reader.ReadUInt32();
            if (csum != 0x04030201)
            {
                throw new Exception("Error: Wrong Checksum!");
            }

            reader.ReadUInt32(); // 01 00 00 00

            int strOffset = reader.ReadInt32();
            // Store this for returning back to the parser
            if (strOffset >= reader.BaseStream.Length)
            {
                throw new ArgumentOutOfRangeException("Error: String table offset out of bounds!");
            }

            long dataPos = reader.BaseStream.Position;
            reader.BaseStream.Position = strOffset;

            // Store this for returning back to the parser
            if (reader.ReadByte() != 0x7)
            {
                throw new Exception("Error: Invalid byte check, expected 0x7!");
            }


            // Iterate over 4-byte string offset table and build LUT
            //ReadOnlySpan<int> offsets = MemoryMarshal.Cast<byte, int>(buffer[strOffset..]);
            int i = 0;
            while((i = reader.ReadInt32()) != -1)
            {
                nameList.Add(i);
            }

            // Restore position before deserializing
            reader.BaseStream.Position = dataPos;
        }

        public GGDictionary Parse()
        {
            ParseHeader();

            GGObject retVal = ParseInternal();

            if (retVal is not GGDictionary)
                throw new Exception("Expected dictionary as root node");

            return (GGDictionary)retVal;
        }
        
        GGObject ParseInternal()
        {
            GGValue type = (GGValue)reader.ReadByte();

            GGObject retVal = type switch
            {
                GGValue.Null => ParseNull(),
                GGValue.Dictionary => ParseDictionary(),
                GGValue.Array => ParseArray(),
                GGValue.Int => ParsePod<int>(),
                GGValue.Float => ParsePod<float>(),
                GGValue.Point or
                GGValue.Rect or
                GGValue.Quad or
                GGValue.String => ParseString(),
                _ => throw new ArgumentOutOfRangeException("Unknown pack type")
            };

            return retVal;
        }

        GGNull ParseNull() => new GGNull();
        
        GGDictionary ParseDictionary()
        {
            uint len = reader.ReadUInt32();
            var dict = new GGDictionary();
            while (len-- > 0)
            {
                ushort nameIndex = reader.ReadUInt16();              
                string key = ReadStringKey(nameIndex);
                
                GGObject value = ParseInternal();
                dict.Add(key, value);
            }

            if (reader.ReadByte() != (byte)GGValue.Dictionary)
                throw new Exception("Illegal array end marker");

            return dict;
        }

        GGArray ParseArray()
        {
            uint len = reader.ReadUInt32();
            GGArray array = new GGArray();
            while (len-- > 0)
            {
                var obj = ParseInternal();
                array.Add(obj);
            }

            if (reader.ReadByte() != (byte)GGValue.Array)
                throw new Exception("Illegal array end marker");

            return array;
        }

        GGObject ParseString()
        {
            ushort index = reader.ReadUInt16();
            string value = ReadStringKey(index);

            return new GGString(value);
        }

        GGPod<T> ParsePod<T>() where T : struct, IConvertible
        {
            ushort index = reader.ReadUInt16();
            string num = ReadStringKey(index);

            T val = (T)Convert.ChangeType(num, typeof(T));

            return new GGPod<T>(val);
        }

        string ReadStringKey(int index)
        {
            long pos = reader.BaseStream.Position;
            reader.BaseStream.Seek(nameList[index], SeekOrigin.Begin);

            var result = new StringBuilder(10);
            while (true)
            {
                byte b = reader.ReadByte();
                if (0 == b)
                    break;
                result.Append((char)b);
            }

            reader.BaseStream.Position = pos;

            return result.ToString();
        }
    }
}
