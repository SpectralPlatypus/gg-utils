using System.Globalization;
using System.Text;

namespace GGUtils
{
    ref struct GGParser
    {
        ReadOnlySpan<byte> buffer;
        int offset = 0;
        List<int> nameList = new();

        public GGParser(ReadOnlySpan<byte> buffer)
        {
            this.buffer = buffer;
        }

        public GGDictionary Parse()
        {
            ParseHeader();

            GGObject retVal = ParseInternal();

            if (retVal is not GGDictionary)
                throw new Exception("Expected dictionary as root node");

            return (GGDictionary)retVal;
        }

        void ParseHeader()
        {
            uint csum = buffer.ReadUInt32(ref offset);
            if (csum != 0x04030201)
            {
                throw new Exception("Error: Wrong Checksum!");
            }

            buffer.ReadUInt32(ref offset); // 01 00 00 00

            int strOffset = buffer.ReadInt32(ref offset);
            if (strOffset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException("Error: String table offset out of bounds!");
            }

            if (buffer.ReadByte(ref strOffset) != 0x7)
            {
                throw new Exception("Error: Invalid byte check, expected 0x7!");
            }


            // Iterate over 4-byte string offset table and build LUT
            int i;
            while ((i = buffer.ReadInt32(ref strOffset)) != -1)
            {
                nameList.Add(i);
            }
        }

        GGObject ParseInternal()
        {
            GGValue type = (GGValue)buffer.ReadByte(ref offset);

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

        GGNull ParseNull() => new();

        GGDictionary ParseDictionary()
        {
            uint len = buffer.ReadUInt32(ref offset);
            GGDictionary dict = new();
            while (len-- > 0)
            {
                ushort nameIndex = buffer.ReadUInt16(ref offset);
                string key = ReadStringKey(nameIndex);

                GGObject value = ParseInternal();
                dict.Add(key, value);
            }

            if (buffer.ReadByte(ref offset) != (byte)GGValue.Dictionary)
                throw new Exception("Illegal array end marker");

            return dict;
        }

        GGArray ParseArray()
        {
            uint len = buffer.ReadUInt32(ref offset);
            GGArray array = new();
            while (len-- > 0)
            {
                var obj = ParseInternal();
                array.Add(obj);
            }

            if (buffer.ReadByte(ref offset) != (byte)GGValue.Array)
                throw new Exception("Illegal array end marker");

            return array;
        }

        GGObject ParseString()
        {
            ushort index = buffer.ReadUInt16(ref offset);
            string value = ReadStringKey(index);

            return new GGString(value);
        }

        GGPod<T> ParsePod<T>() where T : struct, IConvertible
        {
            ushort index = buffer.ReadUInt16(ref offset);
            string num = ReadStringKey(index);

            T val = (T)Convert.ChangeType(num, typeof(T), CultureInfo.InvariantCulture);

            return new GGPod<T>(val);
        }

        string ReadStringKey(int index)
        {
            int idx = nameList[index];
            string? output = buffer.ReadCString(ref idx);

            return output ?? string.Empty;
        }

    }
}
