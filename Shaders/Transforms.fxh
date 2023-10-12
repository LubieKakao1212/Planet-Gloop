
float3x3 ComposeTransform(float4 rotScale, float2 pos)
{
    return float3x3(
		rotScale.x, rotScale.z, pos.x,
		rotScale.y, rotScale.w, pos.y,
		0.f       , 0.f       , 1.f);
}