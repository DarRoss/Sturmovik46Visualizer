using Godot;

public partial class MapData : Node
{
    public static MapData Instance{get; private set;} = null;
    public const int ENTRIES_PER_FIELD = 4;
	public const int METERS_PER_PIXEL = 64;

    public enum Section
    {
        Map = 0,
        Map2d,
        Roads,
        Fields,
        NumSections,
        Unknown
    }
    public enum Submap
    {
        Height = 0,
        Type,
        NumSubmaps,
        // submaps we do not care about
        Irrelevant,
        Unknown
    }
    public enum Fieldmap
    {
        LowLand = 0,
        MidLand,
        Mount,
        Country,
        City,
        AirField,
        Wood,
        Water,
        NumFields,
        Unknown
    }
    public enum Roadmap
    {
        Rail = 0,
        Road,
        Highway,
        NumRoads,
        Unknown
    }

    public readonly PortableCompressedTexture2D[] submaps = 
        new PortableCompressedTexture2D[(int)Submap.NumSubmaps];
    public readonly PortableCompressedTexture2D[,] fieldmaps =
        new PortableCompressedTexture2D[(int)Fieldmap.NumFields, ENTRIES_PER_FIELD];
    public PortableCompressedTexture2D Map2d{get; set;}
    public readonly PortableCompressedTexture2D[] roadmaps =
        new PortableCompressedTexture2D[(int)Roadmap.NumRoads];

    public override void _Ready()
    {
        Instance ??= this;
    }

    public static string SectionToStr(Section section)
    {
        return section switch
        {
            Section.Map => "[MAP]",
            Section.Map2d => "[MAP2D]",
            Section.Fields => "[FIELDS]",
            Section.Roads => "[ROADS]",
            _ => "[UNKNOWN]",
        };
    }

    public static Section GetSectionFromStr(string sectionStr)
    {
        return sectionStr.Trim().ToLower() switch
        {
            "[map]" => Section.Map,
            "[map2d]" => Section.Map2d,
            "[fields]" => Section.Fields,
            "[roads]" => Section.Roads,
            _ => Section.Unknown,
        };
    }

    public static Submap GetSubmapFromStr(string variantStr)
    {
        return variantStr.Trim().ToLower() switch
        {
            "heightmap" => Submap.Height,
            "typemap" => Submap.Type,
            "farmap" or "colormap" or "colourmap" or "smallmap" or "reflmap" => Submap.Irrelevant,
            _ => Submap.Unknown,
        };
    }

    public static Fieldmap GetFieldmapFromStr(string variantStr)
    {
        return variantStr.Trim().ToLower().Remove(variantStr.Length - 1) switch
        {
            "lowland" => Fieldmap.LowLand,
            "midland" => Fieldmap.MidLand,
            "mount" => Fieldmap.Mount,
            "country" => Fieldmap.Country,
            "city" => Fieldmap.City,
            "airfield" => Fieldmap.AirField,
            "wood" => Fieldmap.Wood,
            "water" => Fieldmap.Water,
            _ => Fieldmap.Unknown,
        };
    }

    public static Roadmap GetRoadmapFromStr(string variantStr)
    {
        return variantStr.Trim().ToLower() switch
        {
            "rail" => Roadmap.Rail,
            "road" => Roadmap.Road,
            "highway" => Roadmap.Highway,
            _ => Roadmap.Unknown,
        };
    }

    public bool IsSectionFulfilled(Section section)
    {
        bool output = true;
        int i;
        switch(section)
        {
            case Section.Map:
                for(i = 0; output && i < submaps.Length; ++i)
                {
                    output &= submaps[i] != null;
                }
                break;
            case Section.Map2d:
                output = Map2d != null;
                break;
            case Section.Fields:
                int j;
                bool isFieldFulfilled;
                for(i = 0; output && i < fieldmaps.GetLength(0); ++i)
                {
                    isFieldFulfilled = false;
                    // at least one texture must exist per field
                    for(j = 0; !isFieldFulfilled && j < fieldmaps.GetLength(1); ++j)
                    {
                        isFieldFulfilled |= fieldmaps[i,j] != null;
                    }
                    output &= isFieldFulfilled;
                }
                break;
            case Section.Roads:
                /*
                for(i = 0; output && i < roadmaps.Length; ++i)
                {
                    output &= roadmaps[i] != null;
                }
                */
                break;
        }
        return output;
    }

    public bool AllSectionsFulfilled()
    {
        bool isFulfilled = true;
        for(int i = 0; isFulfilled && i < (int)Section.NumSections; ++i)
        {
            isFulfilled &= IsSectionFulfilled((Section)i);
        }
        return isFulfilled;
    }

    public void ClearSection(Section section)
    {
        int i;
        switch(section)
        {
            case Section.Map:
                for(i = 0; i < submaps.Length; ++i)
                {
                    submaps[i] = null;
                }
                break;
            case Section.Map2d:
                Map2d = null;
                break;
            case Section.Fields:
                int j;
                for(i = 0; i < fieldmaps.GetLength(0); ++i)
                {
                    for(j = 0; j < fieldmaps.GetLength(1); ++j)
                    {
                        fieldmaps[i,j] = null;
                    }
                }
                break;
            case Section.Roads:
                for(i = 0; i < submaps.Length; ++i)
                {
                    roadmaps[i] = null;
                }
                break;
        }
    }
}