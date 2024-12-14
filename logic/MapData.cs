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

    public PortableCompressedTexture2D Map2d{get; set;}
    public readonly PortableCompressedTexture2D[] submaps = 
        new PortableCompressedTexture2D[(int)Submap.NumSubmaps];
    public readonly PortableCompressedTexture2D[,] fieldmaps =
        new PortableCompressedTexture2D[(int)Fieldmap.NumFields, ENTRIES_PER_FIELD];
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
    
    public static string FieldToStr(Fieldmap field)
    {
        return field switch
        {
            Fieldmap.LowLand => "LowLand",
            Fieldmap.MidLand => "MidLand",
            Fieldmap.Mount => "Mount",
            Fieldmap.Country => "Country",
            Fieldmap.City => "City",
            Fieldmap.AirField => "AirField",
            Fieldmap.Wood => "Wood",
            Fieldmap.Water => "Water",
            _ => "Unknown",
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
        int variant;
        switch(section)
        {
            case Section.Map:
                for(variant = 0; output && variant < submaps.Length; ++variant)
                {
                    output &= submaps[variant] != null;
                }
                break;
            case Section.Map2d:
                output = Map2d != null;
                break;
            case Section.Fields:
                int index;
                for(variant = 0; output && variant < fieldmaps.GetLength(0); ++variant)
                {
                    for(index = 0; output && index < fieldmaps.GetLength(1); ++index)
                    {
                        // ignore Wood1 and Wood3
                        if((Fieldmap)variant != Fieldmap.Wood || index == 0 || index == 2)
                        {
                            output &= fieldmaps[variant,index] != null;
                        }
                    }
                }
                break;
            case Section.Roads:
                for(variant = 0; output && variant < roadmaps.Length; ++variant)
                {
                    output &= roadmaps[variant] != null;
                }
                break;
        }
        return output;
    }

    public bool AllSectionsFulfilled()
    {
        bool output = true;
        for(int section = 0; output && section < (int)Section.NumSections; ++section)
        {
            output &= IsSectionFulfilled((Section)section);
        }
        return output;
    }

    public void ClearSection(Section section)
    {
        int variant;
        switch(section)
        {
            case Section.Map:
                for(variant = 0; variant < submaps.Length; ++variant)
                {
                    submaps[variant] = null;
                }
                break;
            case Section.Map2d:
                Map2d = null;
                break;
            case Section.Fields:
                int index;
                for(variant = 0; variant < fieldmaps.GetLength(0); ++variant)
                {
                    for(index = 0; index < fieldmaps.GetLength(1); ++index)
                    {
                        fieldmaps[variant,index] = null;
                    }
                }
                break;
            case Section.Roads:
                for(variant = 0; variant < submaps.Length; ++variant)
                {
                    roadmaps[variant] = null;
                }
                break;
        }
    }
}