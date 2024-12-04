using Godot;

public partial class World : Node3D
{
	private MapLoaderDialog mld;
	public override void _Ready()
	{
		mld = GetNode<MapLoaderDialog>("MapLoaderDialog");
	}

    public override void _Input(InputEvent ie)
    {
		if(ie.IsActionReleased("OpenFile"))
		{
			mld.PopupCenteredRatio();
		}
    }
}
