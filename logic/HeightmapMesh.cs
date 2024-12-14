using Godot;

public partial class HeightmapMesh : MeshInstance3D
{
	private PlaneMesh heightMapPlane;
	public override void _Ready()
	{
		heightMapPlane = (PlaneMesh)Mesh;
	}

	public void UpdateHeightMap()
	{
		PortableCompressedTexture2D heightPct = MapData.Instance.submaps[(int)MapData.Submap.Height];
		ShaderMaterial sMat = heightMapPlane.Material as ShaderMaterial;
		Vector2I textureSize = (Vector2I)heightPct.GetSize();
		heightMapPlane.Size = textureSize * MapData.METERS_PER_PIXEL;
		heightMapPlane.SubdivideWidth = textureSize.X;
		heightMapPlane.SubdivideDepth = textureSize.Y;
		sMat.SetShaderParameter("vHeightmap", heightPct);
		sMat.SetShaderParameter("fTypemap", MapData.Instance.Map2d);

		int field;
		string uniformName;
		PortableCompressedTexture2D[] fieldVariantArr;	
		for(field = 0; field < (int)MapData.Fieldmap.NumFields; ++field)
		{
			uniformName = "f" + MapData.FieldToStr((MapData.Fieldmap)field);
			fieldVariantArr = new PortableCompressedTexture2D[] {
				MapData.Instance.fieldmaps[field, 0],
				MapData.Instance.fieldmaps[field, 1],
				MapData.Instance.fieldmaps[field, 2],
				MapData.Instance.fieldmaps[field, 3]};
			sMat.SetShaderParameter(uniformName, fieldVariantArr);
		}
	}
}
