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

    public readonly PortableCompressedTexture2D[] submaps = 
        new PortableCompressedTexture2D[(int)Submap.NumSubmaps];
    public readonly PortableCompressedTexture2D[,] fieldmaps =
        new PortableCompressedTexture2D[(int)Fieldmap.NumFields, ENTRIES_PER_FIELD];
    public PortableCompressedTexture2D Map2d{get; set;}

    public override void _Ready()
    {
        Instance ??= this;
    }

    public static string SectionToStr(Section section)
    {
        string output = "[UNKNOWN]";
        switch(section)
        {
            case Section.Map:
                output = "[MAP]";
                break;
            case Section.Map2d:
                output = "[MAP2D]";
                break;
            case Section.Fields:
                output = "[FIELDS]";
                break;
        }
        return output;
    }

    public static Section GetSectionFromStr(string sectionStr)
    {
        Section output = Section.Unknown;
        switch(sectionStr.Trim().ToLower())
        {
    		case "[map]":
                output = Section.Map;
    			break;
    		case "[map2d]":
                output = Section.Map2d;
    			break;
            case "[fields]":
                output = Section.Fields;
                break;
        }
        return output;
    }

    public static Submap GetSubmapFromStr(string variantStr)
    {
        Submap output = Submap.Unknown;
        switch(variantStr.Trim().ToLower())
        {
			case "heightmap":
                output = Submap.Height;
				break;
			case "typemap":
                output = Submap.Type;    
				break;
			case "farmap":
			case "colormap":
			case "colourmap":
			case "smallmap":
			case "reflmap":
                output = Submap.Irrelevant;
				break;
        }
        return output;
    }

    public static Fieldmap GetFieldmapFromStr(string variantStr)
    {
        Fieldmap output = Fieldmap.Unknown;
        // remove the last character from the fieldmap variant
		switch(variantStr.Trim().ToLower().Remove(variantStr.Length - 1))
        {
			case "lowland":
                output = Fieldmap.LowLand;
				break;
			case "midland":
                output = Fieldmap.MidLand;
				break;
			case "mount":
                output = Fieldmap.Mount;
				break;
			case "country":
                output = Fieldmap.Country;
				break;
			case "city":
                output = Fieldmap.City;
				break;
			case "airfield":
                output = Fieldmap.AirField;
				break;
			case "wood":
                output = Fieldmap.Wood;
				break;
			case "water":
                output = Fieldmap.Water;
				break;
        }
        return output;
    }

    public bool IsSectionFulfilled(Section section)
    {
        bool output = true;
        switch(section)
        {
            case Section.Map:
                for(int i = 0; output && i < submaps.Length; ++i)
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
                for(int i = 0; output && i < fieldmaps.GetLength(0); ++i)
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
        switch(section)
        {
            case Section.Map:
                for(int i = 0; i < submaps.Length; ++i)
                {
                    submaps[i] = null;
                }
                break;
            case Section.Map2d:
                Map2d = null;
                break;
            case Section.Fields:
                int j;
                for(int i = 0; i < fieldmaps.GetLength(0); ++i)
                {
                    for(j = 0; j < fieldmaps.GetLength(1); ++j)
                    {
                        fieldmaps[i,j] = null;
                    }
                }
                break;
        }
    }
}