using Godot;

public partial class MapData : Node
{
    public enum Variant
    {
        Height = 0,
        Color,
        Type,
        Far,
        NumVariants
    }

    public static MapData Instance{get; private set;} = null;

    public readonly Image[] maps = new Image[(int)Variant.NumVariants];

    public override void _Ready()
    {
        Instance ??= this;
    }

    public bool AllMapsFound()
    {
        bool output = true;
        for(int i = 0; i < maps.Length && output; ++i)
        {
            output &= maps[i] != null;
        }
        return output;
    }
}