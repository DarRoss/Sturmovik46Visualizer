using System.IO;
using Godot;

public partial class MapLoaderDialog : FileDialog
{
	[Signal]
	public delegate void MapLoadedEventHandler();

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
				// read each map variant
	            while (!found && (line = reader.ReadLine()) != null)
	            {
					tokens = line.Split("=");
					// proceed if there is exactly one "=" in the line
					if(!(found = tokens.Length != 2))
					{
						AssignSubMap(tokens[0], tokens[1], dirName);
						found = MapData.Instance.AllSubmapsFound();
					}
	            }
				if(MapData.Instance.AllSubmapsFound())
				{
					EmitSignal(SignalName.MapLoaded);
				}
				else
				{
					GD.PrintErr("Selected INI file did not contain all map types");
				}
			}
			else
			{
				GD.PrintErr("Could not find [MAP] section in selected INI file");
			}
        }
		else
		{
			GD.PrintErr("Selected file does not exist");
		}
	}

	private static void AssignSubMap(string submapVariant, string submapFileName, string dirPath)
	{
		string trimmedSubmapVariant = submapVariant.Trim();
		MapData.Submap variant = MapData.Submap.NumSubmaps;
		switch(trimmedSubmapVariant.ToLower())
		{
			case "colormap":
			case "colourmap":
				variant = MapData.Submap.Color;
				break;
			case "heightmap":
				variant = MapData.Submap.Height;
				break;
			case "typemap":
				variant = MapData.Submap.Type;
				break;
			case "farmap":
				variant = MapData.Submap.Far;
				break;
			case "smallmap":
			case "reflmap":
				// ignore these submaps
				break;
			default:
				GD.Print("Unrecognized submap \"" + trimmedSubmapVariant + "\". Ignoring");
				break;
		}
		if(variant < MapData.Submap.NumSubmaps)
		{
			string trimmedFileName = submapFileName.Trim();
			if(trimmedFileName.EndsWith(".tga"))
			{
				string mapPath = Path.Combine(dirPath, trimmedFileName);
				Image img = new();
				img.Load(mapPath);
				if(img != null)
				{
					PortableCompressedTexture2D pct = new();
					pct.CreateFromImage(img, PortableCompressedTexture2D.CompressionMode.Lossless);
					if(pct != null)
					{
						MapData.Instance.submaps[(int)variant] = pct;
//						GD.Print("Successfully assigned filepath \"" + mapPath + "\"");
					}
					else
					{
						GD.Print("Could not compress image \"" + mapPath + "\". Ignoring");
					}
				}
				else
				{
					GD.Print("Could not load image from path \"" + mapPath + "\". Ignoring");
				}
			}
			else
			{
				GD.Print("File \"" + trimmedFileName + "\" is not a TGA file. Ignoring");
			}
		}
	}
}
