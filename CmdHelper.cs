using System.Text.Json;

namespace GGUtils
{
    static class CmdHelper
    {
       public enum Actions
        {
            List,
            Extract,
            Save,
            None
        }
       public static void ExtractPack(string fileName, string outputPath, string? wildCard)
        {
            GGPack ggPack = new(fileName);
            ggPack.ExtractFiles(outputPath, wildCard);
        }

        public static void ListPack(string fileName, string? wildCard)
        {
            GGPack ggPack = new(fileName);
            var fileList = ggPack.ListFiles(wildCard);
            foreach (var file in fileList)
                Console.WriteLine(file);
        }

        public static void ExtractSave(string fileName, string outputPath)
        {
            if(!File.Exists(fileName) || !Directory.Exists(outputPath))
            {
                Console.WriteLine("Invalid file path, exiting");
            }

            byte[] fileBuf = File.ReadAllBytes(fileName);

            GGDecoder.SaveDecode(fileBuf);

            using BinaryReader br = new(new MemoryStream(fileBuf));
            GGDictionary saveNode = new GGParser(br).Parse();

            outputPath = Path.Combine(outputPath, Path.ChangeExtension(Path.GetFileName(fileName), "json"));

            using FileStream outfs = File.Create(outputPath);
            JsonWriterOptions writerOptions = new() { Indented = true };
            Utf8JsonWriter writer = new Utf8JsonWriter(outfs, writerOptions);
            saveNode.Serialize(writer);
            writer.Flush();

            Console.WriteLine($"Save file written to {outputPath}");
        }

        public static void ShowHelp()
        {
            var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            Console.WriteLine("USAGE: ggutils -l|-x|-s [wildcard] [OPTIONS] [filename]");
            Console.WriteLine("\t-l, --list: List files in archive.");
            Console.WriteLine("\t-s, --save: Save contents of a savefile in JSON format. File name can be a slot number (1-9) or path to the save file.");
            Console.WriteLine("\t-x, --extract: Extract the contents of a ggpack archive.");
            Console.WriteLine(Environment.NewLine + "OPTIONS:");
            Console.WriteLine("\t-f, --filter: Filename filter. Supported for -x and -l commands. Accepts wildcard characters (*, ?)");
            Console.WriteLine("\t-o, --output: Output directory. Supported for -x and -s commands. By default the current execution path will be used.");
            Console.WriteLine(Environment.NewLine + "EXAMPLES:");
            Console.WriteLine("\tPrint the names of all PNG files in archive:");
            Console.WriteLine("\t\tggutils -l -f *.png Weird.ggpack1a");
            Console.WriteLine("\tExtract contents of save slot 3 in parent directory");
            Console.WriteLine("\t\tggutils -s -o ../ 3");

            Environment.Exit(0);
        }
    }
}
