using Godot;

public partial class MapData : Node
{
    public enum Submap
    {
        Height = 0,
        Type,
        NumSubmaps
    }

    public static MapData Instance{get; private set;} = null;

    public readonly PortableCompressedTexture2D[] submaps = 
        new PortableCompressedTexture2D[(int)Submap.NumSubmaps];
    public PortableCompressedTexture2D Map2d{get; set;}

    public override void _Ready()
    {
        Instance ??= this;
    }

    public bool AllSubmapsFound()
    {
        bool output = true;
        for(int i = 0; output && i < submaps.Length; ++i)
        {
            output &= submaps[i] != null;
        }
        return output;
    }

    public void ClearSubmaps()
    {
        for(int i = 0; i < submaps.Length; ++i)
        {
            submaps[i] = null;
        }
    }
}