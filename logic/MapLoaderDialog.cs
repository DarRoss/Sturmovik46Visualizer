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
            while ((!submapSuccess || !map2dSuccess) && (line = reader.ReadLine().Trim()) != null)
            {
				switch(line.ToLower())
				{
					case "[map]":
						submapSuccess = ReadSection(reader, iniPath, MapData.Section.Map);
						break;
					case "[map2d]":
						map2dSuccess = ReadSection(reader, iniPath, MapData.Section.Map2d);
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

	private static bool ReadSection(StreamReader reader, string iniPath, MapData.Section section)
	{
		string dirPath = Path.GetDirectoryName(iniPath);
		string[] tokens;
		string line;
		bool breakWhile = false;
		bool success = false;

		// clear any previously loaded section data
		MapData.Instance.ClearSection(section);
		// read lines in this section
        while (!success && !breakWhile && (line = reader.ReadLine().Trim()) != null)
        {
			// ignore commented lines
			if(line.Length > 0 && line[0] != ';' && line[0] != '/')
			{
				tokens = line.Split("=");
				AssignSectionEntry(section, dirPath, tokens);
				success = MapData.Instance.IsSectionFulfilled(section);
			}
			// do not read the next line if it is the start of another section
			breakWhile = reader.Peek() == '[';
        }
		return success;
	}

	private static void AssignSectionEntry(MapData.Section section, string dirPath, string[] tokens)
	{
		MapData.Submap submapVariant = MapData.Submap.NumSubmaps;
		MapData.Fieldmap fieldVariant = MapData.Fieldmap.NumFields;
		bool sectionIsValid = false;
		string sectionStr = "[UNKNOWN SECTION]";
		string fileName = "";
		string variantStr;

		switch(section)
		{
			case MapData.Section.Map:
				sectionStr = "MAP";
				if(tokens.Length == 2)
				{
					variantStr = tokens[0].Trim();
					fileName = tokens[1].Trim();
					switch(variantStr.ToLower())
					{
						case "heightmap":
							submapVariant = MapData.Submap.Height;
							break;
						case "typemap":
							submapVariant = MapData.Submap.Type;
							break;
						case "farmap":
						case "colormap":
						case "colourmap":
						case "smallmap":
						case "reflmap":
							// ignore these submaps
							break;
						default:
							GD.Print("[MAP]: Unrecognized submap variant \"" + variantStr + "\". Ignoring");
							break;
					}
					sectionIsValid = submapVariant < MapData.Submap.NumSubmaps;
				}
				break;
			case MapData.Section.Map2d:
				sectionStr = "MAP2D";
				if(sectionIsValid = tokens.Length == 1)
				{
					fileName = tokens[0].Trim();
				}
				break;
			case MapData.Section.Fields:
				sectionStr = "FIELDS";
				if(tokens.Length == 2)
				{
					// TODO
				}
				break;
		}

		if(sectionIsValid)
		{
			if(fileName.EndsWith(".tga"))
			{
				string tgaPath = Path.Combine(dirPath, fileName);
				Image img = new();
				img.Load(tgaPath);
				if(img != null)
				{
					PortableCompressedTexture2D pct = new();
					pct.CreateFromImage(img, PortableCompressedTexture2D.CompressionMode.Lossless);
					if(pct != null)
					{
						switch(section)
						{
							case MapData.Section.Map2d:
								MapData.Instance.Map2d = pct;
								break;
							case MapData.Section.Map:
								MapData.Instance.submaps[(int)submapVariant] = pct;
								break;
							case MapData.Section.Fields:
								// TODO
								break;
						}
						GD.Print("[" + sectionStr + "]: successfully assigned image \"" + tgaPath + "\"");
					}
					else
					{
						GD.Print("[" + sectionStr + "]: could not compress image \"" + tgaPath + "\". Ignoring");
					}
				}
				else
				{
					GD.Print("[" + sectionStr + "]: could not load image from path \"" + tgaPath + "\". Ignoring");
				}
			}
			else
			{
				GD.Print("[" + sectionStr + "]: file \"" + fileName + "\" is not a TGA file. Ignoring");
			}
		}
	}
}
