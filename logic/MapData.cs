using Godot;

public partial class MapData : Node
{
    public enum Submap
    {
        Height = 0,
        Color,
        Type,
        Far,
        NumSubmaps
    }

    public static MapData Instance{get; private set;} = null;

    public readonly PortableCompressedTexture2D[] submaps = 
        new PortableCompressedTexture2D[(int)Submap.NumSubmaps];

    public override void _Ready()
    {
        Instance ??= this;
    }

    public bool AllSubmapsFound()
    {
        bool output = true;
        for(int i = 0; i < submaps.Length && output; ++i)
        {
            output &= submaps[i] != null;
        }
        return output;
    }
}