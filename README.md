# GGUtils

This tools allows data/file extraction from saves files and archives (.ggpack) used by the game [Return to Monkey Island](https://returntomonkeyisland.com/).

Aim of the project was largely re-learning C# (and its newer features), expect strange conventions and jank.

## Dependencies

* [Mono.Options](https://www.nuget.org/packages/Mono.Options/)
* [BCnEncoder.NET](https://github.com/Nominom/BCnEncoder.NET)
* [ImageSharp](https://github.com/SixLabors/ImageSharp)

## Building & Running

* Building executable:
	* `dotnet publish -r <platform> -c Release -p:PublishSingleFile=true --self-contained false`
	* Invoke exe via CLI
* Alternatively :
	* `dotnet run <commands>`

## Usage

```
USAGE: ggutils -l|-x|-s [OPTIONS] [filename]
        -l, --list: List files in archive.
        -s, --save: Save contents of a savefile in JSON format. File name can be a slot number (1-9) or path to the save file.
        -x, --extract: Extract the contents of a ggpack archive.

OPTIONS:
        -f, --filter: Filename filter. Supported for -x and -l commands. Accepts wildcard characters (*, ?)
        -o, --output: Output directory. Supported for -x and -s commands. By default the current execution path will be used.

EXAMPLES:
        Print the names of all PNG files in archive:
               ggutils -l -f *.png Weird.ggpack1a
        Extract contents of save slot 3 in parent directory
                ggutils -s -o ../ 3
```

## Thanks

The project https://github.com/scemino/ngpack was used as a reference for handling ggpack archives
