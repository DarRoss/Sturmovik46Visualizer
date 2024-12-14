using System.IO;
using Godot;

public partial class MapLoaderDialog : FileDialog
{
	private struct EntryData
	{
		public string FileName{get; set;}
		public string VariantStr{get; set;}
		public int VariantIndex{get; set;}
		public int FieldIndex{get; set;}
    }

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
				GD.Print("MAPLOADER:\tMap successfully loaded.");
			}
			else
			{
				GD.Print("MAPLOADER:\tMap failed to load due to unfulfilled sections.");
			}
        }
		else
		{
			GD.PrintErr("FILEDIALOG:\tSelected file does not exist");
		}
	}

	private static bool ReadSectionLines(StreamReader reader, string iniPath, MapData.Section section)
	{
		string dirPath = Path.GetDirectoryName(iniPath);
		string[] tokens;
		string line;
		bool breakWhile = false;
		bool success = false;
		EntryData data = new();
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
				if(IsSectionEntryValid(section, tokens, ref data))
				{
					AssignSectionEntry(section, dirPath, ref data);
				}
				success = MapData.Instance.IsSectionFulfilled(section);
			}
			// do not read the next line if it is the start of another section
			breakWhile = reader.Peek() == '[';
        }
		return success;
	}

	private static bool IsSectionEntryValid(MapData.Section section, string[] tokens, ref EntryData data)
	{
		bool output = false;
		switch(section)
		{
			case MapData.Section.Map:
				if(tokens.Length == 2)
				{
					data.VariantStr = tokens[0];
					data.FileName = TrimDelimitersFromFileName(tokens[1]);
					data.VariantIndex = (int)MapData.GetSubmapFromStr(data.VariantStr);
					if(data.VariantIndex == (int)MapData.Submap.Unknown)
					{
						GD.Print("[MAP]:\tUnrecognized submap variant \"" + data.VariantStr + "\". Ignoring");
					}
					output = data.VariantIndex < (int)MapData.Submap.NumSubmaps;
				}
				break;
			case MapData.Section.Map2d:
				if(output = tokens.Length == 1)
				{
					data.FileName = TrimDelimitersFromFileName(tokens[0]);
				}
				break;
			case MapData.Section.Fields:
				if(tokens.Length == 2)
				{
					data.FileName = TrimDelimitersFromFileName(tokens[1]);
					data.VariantStr = tokens[0];
					data.VariantIndex = (int)MapData.GetFieldmapFromStr(data.VariantStr);
					if(data.VariantIndex == (int)MapData.Fieldmap.Unknown)
					{
						GD.Print("[FIELDS]:\tUnknown field variant \"" + data.VariantStr + "\". Ignoring");
					}
					if(data.VariantIndex < (int)MapData.Fieldmap.NumFields)
					{
						// convert last char to index number
						data.FieldIndex = data.VariantStr[^1] - '0';
						// field index must be within field variant array index bounds
						output = data.FieldIndex < MapData.ENTRIES_PER_FIELD && data.FieldIndex >= 0;
					}
				}
				break;
			case MapData.Section.Roads:
				if(tokens.Length == 2)
				{
					data.VariantStr = tokens[0];
					// files listed in [ROADS] omit file extension. Re-add it
					data.FileName = TrimDelimitersFromFileName(tokens[1]) + ".tga";
					data.VariantIndex = (int)MapData.GetRoadmapFromStr(data.VariantStr);
					if(data.VariantIndex == (int)MapData.Roadmap.Unknown)
					{
						GD.Print("[ROADS]:\tUnrecognized road variant \"" + data.VariantStr + "\". Ignoring");
					}
					output = data.VariantIndex < (int)MapData.Roadmap.NumRoads;
				}
				break;
		}
		return output;
	}

	private static void AssignSectionEntry(MapData.Section section, string dirPath, ref EntryData data)
	{
		string sectionStr = MapData.SectionToStr(section);
		Image img = new();
		string imgPath;
		if(section == MapData.Section.Map || section == MapData.Section.Map2d)
		{
			// image is found in same directory as load.ini
			imgPath = Path.Combine(dirPath, data.FileName);
		}
		else
		{
			// image is found in "_Tex" directory
			imgPath = Path.Combine(Path.GetDirectoryName(dirPath), "_Tex", data.FileName);
		}
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
						MapData.Instance.submaps[data.VariantIndex] = pct;
						break;
					case MapData.Section.Fields:
						MapData.Instance.fieldmaps[data.VariantIndex, data.FieldIndex] = pct;
						break;
					case MapData.Section.Roads:
						MapData.Instance.roadmaps[data.VariantIndex] = pct;
						break;
				}
				GD.Print(sectionStr + ":\tsuccessfully assigned image \"" + imgPath + "\"");
			}
			else
			{
				GD.Print(sectionStr + ":\tfailed to compress image \"" + imgPath + "\". Ignoring");
			}
		}
		else
		{
			GD.Print(sectionStr + ":\tfailed to load image \"" + imgPath + "\". Ignoring");
			if(section != MapData.Section.Map2d)
			{
				GD.Print("\tVariant name: " + data.VariantStr);
			}
		}
	}

	private static string TrimDelimitersFromFileName(string fileName)
	{
		string output;
		// look for commas in filename
		int commaIndex = fileName.IndexOf(',');
		if(commaIndex > -1)
		{
			// take substring prior to comma
			output = fileName[..commaIndex].Trim();
		}
		else
		{
			// look for comments in filename
			int commentIndex = fileName.IndexOf("//");
			if(commentIndex > -1)
			{
				// take substring prior to comment
				output = fileName[..commentIndex].Trim();
			}
			else
			{
				output = fileName;
			}
		}
		return output;
	}
}
