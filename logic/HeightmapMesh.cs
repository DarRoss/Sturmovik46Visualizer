using Godot;

public partial class HeightmapMesh : MeshInstance3D
{
	private const int DIVISION_MODIFIER = 2;

	private PlaneMesh heightMapPlane;
	public override void _Ready()
	{
		heightMapPlane = (PlaneMesh)Mesh;
	}

	public void UpdateHeightMap()
	{
		PortableCompressedTexture2D heightPct = MapData.Instance.submaps[(int)MapData.Submap.Height];
		PortableCompressedTexture2D farPct = MapData.Instance.submaps[(int)MapData.Submap.Far];
		Vector2I textureSize = (Vector2I)heightPct.GetSize();
		heightMapPlane.Size = textureSize;
		heightMapPlane.SubdivideWidth = textureSize.X / DIVISION_MODIFIER;
		heightMapPlane.SubdivideDepth = textureSize.Y / DIVISION_MODIFIER;
		(heightMapPlane.Material as ShaderMaterial).SetShaderParameter("VertHeightmap", heightPct);
		(heightMapPlane.Material as ShaderMaterial).SetShaderParameter("FragFarmap", farPct);
	}
}