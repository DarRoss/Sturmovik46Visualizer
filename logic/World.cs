using Godot;

public partial class World : Node3D
{
	private MapLoaderDialog mld;
	private HeightmapMesh hmm;

	public override void _Ready()
	{
		mld = GetNode<MapLoaderDialog>("MapLoaderDialog");
		hmm = GetNode<HeightmapMesh>("HeightmapMesh");
		Input.MouseMode = Input.MouseModeEnum.Visible;
		mld.MapLoaded += hmm.UpdateHeightMap;
	}

    public override void _Input(InputEvent ie)
    {
		if(ie.IsActionReleased("OpenFile"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			mld.PopupCenteredRatio();
		}
		if(ie.IsActionReleased("MouseCaptureToggle"))
		{
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Visible ? 
				Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
		}
    }
}
