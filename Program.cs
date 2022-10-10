using GGUtils;
using Mono.Options;

string? wildCard = null;
string outputPath = Environment.CurrentDirectory;
CmdHelper.Actions cmdAction = CmdHelper.Actions.None;

var options = new OptionSet
            {
                {"h|?|help", "Show this help message", v => CmdHelper.ShowHelp()},
                {"l|list", "List files in a .ggpack archive", v => cmdAction = CmdHelper.Actions.List },
                {"x|extract", "Extract contents of a .ggpack archive. " +
                "When given, only files matching the filter will be listed", v => cmdAction = CmdHelper.Actions.Extract },
                {"s|save", "Extract contents of a save slot <1-9>. Output is in JSON format", v => cmdAction = CmdHelper.Actions.Save },

                {"f=|filter=", "Optional filter for archive list and extract" +
                " When given, only files matching the filter will be processed", v => wildCard = v },
                {"o=|output=", "Optional argument to specify output directory. By default, the current directory will be used", v => outputPath = v}
            };

var cmdArgs = options.Parse(args);

if(cmdAction == CmdHelper.Actions.None)
{
    Console.WriteLine("One of -l, -s, -x actions must be specified!");
    CmdHelper.ShowHelp();
}

string fileName = args[args.Length-1];

switch(cmdAction)
{
    case CmdHelper.Actions.Extract: 
        CmdHelper.ExtractPack(fileName, outputPath, wildCard);
        break;
    case CmdHelper.Actions.List:
        CmdHelper.ListPack(fileName, wildCard);
        break;
    case CmdHelper.Actions.Save:
        {
            if (int.TryParse(fileName, out int slotNumber))
            {
                if(slotNumber < 1 || slotNumber > 9)
                    Console.WriteLine("Save slot id must be between 1-9!");
                else 
                    fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Terrible Toybox", "Return to Monkey Island", $"Savegame{slotNumber}.save");
            }

            CmdHelper.ExtractSave(fileName, outputPath);
        }
        break;
    default:
        break;
}