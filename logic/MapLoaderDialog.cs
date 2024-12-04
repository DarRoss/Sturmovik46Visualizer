using System.IO;
using Godot;

public partial class MapLoaderDialog : FileDialog
{
    public override void _Ready()
    {
		FileMode = FileModeEnum.OpenFile;
		Access = AccessEnum.Filesystem;
		AddFilter("*.ini", "INI Files");
		Title = "Open an INI File";
		FileSelected += OpenLoadIni;
    }

	private void OpenLoadIni(string iniPath)
	{
		if(File.Exists(iniPath))
		{
            using StreamReader reader = new(iniPath);
			string dirName = Path.GetDirectoryName(iniPath);
            string line;
			string[] tokens;
			bool found = false;
			// look for [MAP] section
            while (!found && (line = reader.ReadLine()) != null)
            {
				found = line.Trim().ToLower().Equals("[map]");
            }
			// check if we've found the [MAP] section
			if(found)
			{
				found = false;
				// read each map type
	            while (!found && (line = reader.ReadLine()) != null)
	            {
					tokens = line.Split("=");
					// proceed if there is exactly one "=" in the line
					if(!(found = tokens.Length != 2))
					{
						AssignPossibleMap(tokens[0], tokens[1], dirName);
						found = MapData.Instance.AllMapsFound();
					}
	            }
				if(!MapData.Instance.AllMapsFound())
				{
					GD.PrintErr("INI file did not contain all map types");
				}
			}
			else
			{
				GD.PrintErr("Could not find [MAP] section in INI file");
			}
        }
		else
		{
			GD.PrintErr("File does not exist");
		}
	}

	private static void AssignPossibleMap(string mapType, string mapFileName, string dirPath)
	{
		string trimmedName = mapFileName.Trim();
		if(trimmedName.EndsWith(".tga"))
		{
			string trimmedType = mapType.Trim();
			MapData.Variant variant = MapData.Variant.NumVariants;
			switch(trimmedType.ToLower())
			{
				case "colormap":
				case "colourmap":
					variant = MapData.Variant.Color;
					break;
				case "heightmap":
					variant = MapData.Variant.Height;
					break;
				case "typemap":
					variant = MapData.Variant.Type;
					break;
				case "farmap":
					variant = MapData.Variant.Far;
					break;
				case "smallmap":
				case "reflmap":
					// ignore these map types
					break;
				default:
					GD.Print("Unrecognized map type \"" + trimmedType + "\". Ignoring");
					break;
			}
			if(variant < MapData.Variant.NumVariants)
			{
				string mapPath = Path.Combine(dirPath, trimmedName);
				Image image = Image.LoadFromFile(mapPath);
				if(image != null)
				{
					MapData.Instance.maps[(int)variant] = image;
					GD.Print("Successfully assigned filepath \"" + mapPath + "\"");
				}
				else
				{
					GD.Print("Could not load image from path \"" + mapPath + "\". Ignoring");
				}
			}
		}
		else
		{
			GD.Print("File \"" + trimmedName + "\" is not a TGA file. Ignoring");
		}
	}
}
