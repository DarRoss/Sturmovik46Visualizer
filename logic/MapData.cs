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
        NumSections
    }
    public enum Submap
    {
        Height = 0,
        Type,
        NumSubmaps
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
        NumFields
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
                // TODO
                output = false;
                break;
        }
        return output;
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