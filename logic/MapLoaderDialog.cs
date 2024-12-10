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
		FileSelected += ReadIniSections;
		UseNativeDialog = true;
    }

	private void ReadIniSections(string iniPath)
	{
		if(File.Exists(iniPath))
		{
            using StreamReader reader = new(iniPath);
            string line;
			MapData.Section section;
            while ((line = reader.ReadLine()) != null)
            {
				line = line.Trim();
				section = MapData.GetSectionFromStr(line);
				if(section != MapData.Section.Unknown)
				{
					ReadSectionLines(reader, iniPath, section);
				}
            }
			if(MapData.Instance.AllSectionsFulfilled())
			{
				EmitSignal(SignalName.MapLoaded);
			}
        }
		else
		{
			GD.PrintErr("Selected file does not exist");
		}
	}

	private static bool ReadSectionLines(StreamReader reader, string iniPath, MapData.Section section)
	{
		string dirPath = Path.GetDirectoryName(iniPath);
		string[] tokens;
		string line;
		bool breakWhile = false;
		bool success = false;

		// clear any previously loaded section data
		MapData.Instance.ClearSection(section);
		// read lines in this section
        while (!success && !breakWhile && (line = reader.ReadLine()) != null)
        {
			line = line.Trim();
			// ignore commented lines
			if(line.Length > 0 && line[0] != ';' && line[0] != '/')
			{
				tokens = line.Split("=", System.StringSplitOptions.TrimEntries 
					| System.StringSplitOptions.RemoveEmptyEntries);
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
		MapData.Submap submapVariant = MapData.Submap.Unknown;
		MapData.Fieldmap fieldVariant = MapData.Fieldmap.Unknown;
		int fieldIndex = -1;
		bool variantIsValid = false;
		string fileName = "";
		string variantStr = "UNKNOWN";

		switch(section)
		{
			case MapData.Section.Map:
				if(tokens.Length == 2)
				{
					variantStr = tokens[0];
					fileName = tokens[1];
					submapVariant = MapData.GetSubmapFromStr(variantStr);
					if(submapVariant == MapData.Submap.Unknown)
					{
						GD.Print("[MAP]: Unrecognized submap variant \"" + variantStr + "\". Ignoring");
					}
					variantIsValid = submapVariant < MapData.Submap.NumSubmaps;
				}
				break;
			case MapData.Section.Map2d:
				if(variantIsValid = tokens.Length == 1)
				{
					fileName = tokens[0];
				}
				break;
			case MapData.Section.Fields:
				if(tokens.Length == 2)
				{
					// look for commas in filename
					int commaIndex = tokens[1].IndexOf(',');
					if(commaIndex != -1)
					{
						// take substring prior to comma
						fileName = tokens[1][..commaIndex].Trim();
					}
					else
					{
						// look for comments in filename
						int commentIndex = tokens[1].IndexOf("//");
						if(commentIndex != -1)
						{
							// take substring prior to comment
							fileName = tokens[1][..commentIndex].Trim();
						}
						else
						{
							fileName = tokens[1];
						}
					}
					variantStr = tokens[0];
					fieldVariant = MapData.GetFieldmapFromStr(variantStr);
					if(fieldVariant == MapData.Fieldmap.Unknown)
					{
						GD.Print("[FIELDS]: Unknown field variant \"" + variantStr + "\". Ignoring");
					}
					if(fieldVariant < MapData.Fieldmap.NumFields)
					{
						// convert last char to index number
						fieldIndex = variantStr[^1] - '0';
						// field index must be within field variant array index bounds
						variantIsValid = fieldIndex < MapData.ENTRIES_PER_FIELD && fieldIndex >= 0;
					}
				}
				break;
		}
		if(variantIsValid)
		{
			string sectionStr = MapData.SectionToStr(section);
			if(fileName.EndsWith(".tga"))
			{
				string imgPath;
				if(section == MapData.Section.Fields)
				{
					// image is found in "_Tex" dir
					imgPath = Path.Combine(Path.GetDirectoryName(dirPath), "_Tex", fileName);
				}
				else
				{
					// image is found in same dir as load.ini
					imgPath = Path.Combine(dirPath, fileName);
				}
				Image img = new();
				if(img.Load(imgPath) == Error.Ok)
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
								MapData.Instance.fieldmaps[(int)fieldVariant, fieldIndex] = pct;
								break;
						}
						GD.Print(sectionStr + ": successfully assigned image \"" + imgPath + "\"");
					}
					else
					{
						GD.Print(sectionStr + ": failed to compress image \"" + imgPath + "\". Ignoring");
					}
				}
				else
				{
					GD.Print(sectionStr + ": failed to load image \"" + imgPath + "\". Ignoring");
					if(section != MapData.Section.Map2d)
					{
						GD.Print("\tVariant name: " + variantStr);
					}
				}
			}
			else
			{
				GD.Print(sectionStr + ": file \"" + fileName + "\" is not a TGA file. Ignoring");
			}
		}
	}
}
