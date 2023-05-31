//Requires Transforms.fxh

float4 CameraRS;
float2 CameraT;

float3x3 Projection()
{
	return ComposeTransform(CameraRS, CameraT);
}

float3x3 LocalToView(float4 rotScale, float2 pos)
{
	float3x3 camTRS = Projection();

	float3x3 trs = ComposeTransform(rotScale, pos);

	return mul(camTRS, trs);
}