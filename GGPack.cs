using System.IO.Compression;
using System.IO.Enumeration;
using System.Text.Json;
using BCnEncoder.Decoder;
using BCnEncoder.Shared.ImageFiles;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using BCnEncoder.Shared;

namespace GGUtils
{
    internal class GGPack
    {
        readonly BinaryReader reader;
        readonly GGArray fileList;

        public GGPack(string fileName)
        {
            if(!File.Exists(fileName))
            {
                throw new FileNotFoundException();
            }

            reader = new(File.OpenRead(fileName));

            // Retrieve table offset and decode
            uint offs = reader.ReadUInt32();
            int len = reader.ReadInt32();

            reader.BaseStream.Seek(offs, SeekOrigin.Begin);
            byte[] backingBuffer = reader.ReadBytes(len);
            Span<byte> bSpan = new(backingBuffer);

            GGDecoder.GGPackDecode(backingBuffer);
            GGObject retval = new GGParser(bSpan).Parse()["files"];

            if (retval is GGArray array)
                fileList = array;
            else
                throw new InvalidCastException("Expected files array");
        }

        public List<string> ListFiles(string? wildCard = null)
        {
            List<string> retList = new();
            foreach(var file in fileList)
            {
                if (file is GGDictionary f)
                {
                    if (f["filename"] is not GGString name ||
                        (!string.IsNullOrEmpty(wildCard) &&
                        !FileSystemName.MatchesSimpleExpression(wildCard, name.Value)))
                        continue;

                    retList.Add(name.Value);
                }
            }

            return retList;
        }

        public void ExtractFiles(string outputPath, string? wildCard = null)
        {
            if (Directory.GetParent(outputPath)!.Exists && !Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            foreach (var file in fileList)
            {
                if (file is GGDictionary f)
                {
                    // Check all 3 types are valid and that wildcard matches (if provided)
                    if (f["filename"] is not GGString name || 
                        f["size"] is not GGPod<int> size ||
                        f["offset"] is not GGPod<int> offset ||
                        (!string.IsNullOrEmpty(wildCard) &&
                        !FileSystemName.MatchesSimpleExpression(wildCard, name.Value)))
                        continue;

                    string output = Path.Combine(outputPath, name.ToString());
                    ExtractFile(size.Value, offset.Value, output);
                }
            }
        }

        void ExtractFile(int size, int offset, string outputPath)
        {
            reader.BaseStream.Position = offset;

            byte[] buffer = reader.ReadBytes(size);
            GGDecoder.GGPackDecode(buffer);


            string ext = Path.GetExtension(outputPath);

            /*
             * TODO: What even is yack?
             */

            // JSON and Wimpy are stored as serialized GGObjects
            if (ext == ".json" || ext == ".wimpy")
            {
                using var fs = File.Create(outputPath);

                GGDictionary payload = new GGParser(buffer).Parse();

                JsonWriterOptions writerOptions = new() { Indented = true };
                using Utf8JsonWriter writer = new(fs, writerOptions);

                payload.Serialize(writer);
                writer.Flush();
            }
            else if (ext == ".ktxbz")
            {
                outputPath = Path.ChangeExtension(outputPath, "png");
                using MemoryStream outFs = new();

                using ZLibStream zStream = new(new MemoryStream(buffer), CompressionMode.Decompress);
                zStream.CopyTo(outFs);
                outFs.Position = 0;

                BcDecoder decode = new();
                var ktx = KtxFile.Load(outFs);
                var image = decode.Decode(ktx);

                int width = (int)ktx.MipMaps[0].Width;
                int height = (int)ktx.MipMaps[0].Height;

                var byteArray = MemoryMarshal.AsBytes<ColorRgba32>(image);

                var bmp = Image.LoadPixelData<Rgba32>(byteArray, width, height);
                bmp.SaveAsPng(outputPath);
            }
            else //Plain files (txt, png, atlas etc)
            {
                using var fs = File.Create(outputPath);
                fs.Write(buffer, 0, size);
            }
        }
    }
}
