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
		UseNativeDialog = true;
    }

	private void OpenLoadIni(string iniPath)
	{
		if(File.Exists(iniPath))
		{
            using StreamReader reader = new(iniPath);
            string line;
			bool submapSuccess = false;
			bool map2dSuccess = false;
            while ((!submapSuccess || !map2dSuccess) && (line = reader.ReadLine()) != null)
            {
				switch(line.Trim().ToLower())
				{
					case "[map]":
						submapSuccess = ReadSubmaps(reader, iniPath);
						break;
					case "[map2d]":
						map2dSuccess = ReadMap2d(reader, iniPath);
						break;
				}

            }
			if(submapSuccess && map2dSuccess)
			{
				EmitSignal(SignalName.MapLoaded);
			}
        }
		else
		{
			GD.PrintErr("Selected file does not exist");
		}
	}

	private static bool ReadSubmaps(StreamReader reader, string iniPath)
	{
		string dirPath = Path.GetDirectoryName(iniPath);
		string[] tokens;
		string line;
		bool breakWhile = false;
		bool success = false;

		// clear any previously loaded submap data
		MapData.Instance.ClearSubmaps();
		// read each submap variant
        while (!success && !breakWhile && (line = reader.ReadLine()) != null)
        {
			// ignore commented lines
			if(line.Length > 0 && line[0] != ';')
			{
				tokens = line.Split("=");
				// only proceed if there is exactly one '='. otherwise break
				if(!(breakWhile = tokens.Length != 2))
				{
					if(AssignSubMap(tokens[0].Trim(), tokens[1].Trim(), dirPath))
					{
						success = MapData.Instance.AllSubmapsFound();
					}
					// do not read the next line if it is the start of another section
					breakWhile = reader.Peek() == '[';
				}
			}
        }
		return success;
	}

	private static bool ReadMap2d(StreamReader reader, string iniPath)
	{
		string dirPath = Path.GetDirectoryName(iniPath);
		string line;
		bool breakWhile = false;
		bool success = false;

		// clear any previously loaded map2D data
		MapData.Instance.Map2d = null;
		// read each submap variant
        while (!success && !breakWhile && (line = reader.ReadLine()) != null)
        {
			// ignore commented lines
			if(line.Length > 0 && line[0] != ';')
			{
				success = AssignMap2d(line, dirPath);
				// do not read the next line if it is the start of another section
				breakWhile = reader.Peek() == '[';
			}
        }
		return success;
	}

	private static bool AssignSubMap(string trimmedVariant, string trimmedFileName, string dirPath)
	{
		bool success = false;
		MapData.Submap variant = MapData.Submap.NumSubmaps;
		switch(trimmedVariant.ToLower())
		{
			case "heightmap":
				variant = MapData.Submap.Height;
				break;
			case "typemap":
				variant = MapData.Submap.Type;
				break;
			case "farmap":
			case "colormap":
			case "colourmap":
			case "smallmap":
			case "reflmap":
				// ignore these submaps
				break;
			default:
				GD.Print("Unrecognized submap \"" + trimmedVariant + "\". Ignoring");
				break;
		}
		if(variant < MapData.Submap.NumSubmaps)
		{
			if(trimmedFileName.EndsWith(".tga"))
			{
				string mapPath = Path.Combine(dirPath, trimmedFileName);
				Image img = new();
				img.Load(mapPath);
				if(img != null)
				{
					PortableCompressedTexture2D pct = new();
					pct.CreateFromImage(img, PortableCompressedTexture2D.CompressionMode.Lossless);
					if(success = pct != null)
					{
						MapData.Instance.submaps[(int)variant] = pct;
						GD.Print("Successfully assigned submap filepath \"" + mapPath + "\"");
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
				GD.Print("Submap file \"" + trimmedFileName + "\" is not a TGA file. Ignoring");
			}
		}
		return success;
	}

	private static bool AssignMap2d(string trimmedFileName, string dirPath)
	{
		bool success = false;
		if(trimmedFileName.EndsWith(".tga"))
		{
			string map2dPath = Path.Combine(dirPath, trimmedFileName);
			Image img = new();
			img.Load(map2dPath);
			if(img != null)
			{
				PortableCompressedTexture2D pct = new();
				pct.CreateFromImage(img, PortableCompressedTexture2D.CompressionMode.Lossless);
				if(success = pct != null)
				{
					MapData.Instance.Map2d = pct;
					GD.Print("Successfully assigned map2D filepath \"" + map2dPath + "\"");
				}
				else
				{
					GD.Print("Could not compress image \"" + map2dPath + "\". Ignoring");
				}
			}
			else
			{
				GD.Print("Could not load image from path \"" + map2dPath + "\". Ignoring");
			}
		}
		else
		{
			GD.Print("Map2D file \"" + trimmedFileName + "\" is not a TGA file. Ignoring");
		}
		return success;
	}
}
