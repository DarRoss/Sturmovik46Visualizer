using Godot;

public partial class HeightmapMesh : MeshInstance3D
{
	private const int DIVISION_MODIFIER = 1;

	private PlaneMesh heightMapPlane;
	public override void _Ready()
	{
		heightMapPlane = (PlaneMesh)Mesh;
		(heightMapPlane.Material as ShaderMaterial).SetShaderParameter("MetersPerPixel", MapData.METERS_PER_PIXEL);
	}

	public void UpdateHeightMap()
	{
		PortableCompressedTexture2D heightPct = MapData.Instance.submaps[(int)MapData.Submap.Height];
		ShaderMaterial sMat = heightMapPlane.Material as ShaderMaterial;
		Vector2I textureSize = (Vector2I)heightPct.GetSize();
		heightMapPlane.Size = textureSize * MapData.METERS_PER_PIXEL;
		heightMapPlane.SubdivideWidth = textureSize.X / DIVISION_MODIFIER;
		heightMapPlane.SubdivideDepth = textureSize.Y / DIVISION_MODIFIER;
		sMat.SetShaderParameter("vHeightmap", heightPct);
		sMat.SetShaderParameter("fTypemap", MapData.Instance.Map2d);
		sMat.SetShaderParameter("fWater", MapData.Instance.fieldmaps[(int)MapData.Fieldmap.Water, 1]);
		sMat.SetShaderParameter("fLand", MapData.Instance.fieldmaps[(int)MapData.Fieldmap.LowLand, 3]);
	}
}
