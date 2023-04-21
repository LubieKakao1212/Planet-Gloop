#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 CameraRS;
float2 CameraT;

struct VertexShaderInput
{
	float2 Position : POSITION;
	float4 rotScale : TEXCOORD0;
	float2 pos : TEXCOORD1;
	float4 color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

float3x3 LocalToView(float4 rotScale, float2 pos)
{
	float3x3 camTRS = float3x3(
		CameraRS.x, CameraRS.z, CameraT.x,
		CameraRS.y, CameraRS.w, CameraT.y,
		0         , 0         , 1);

	float3x3 trs = float3x3(
		rotScale.x, rotScale.z, pos.x,
		rotScale.y, rotScale.w, pos.y,
		0         , 0         , 1);

	return mul(camTRS, trs);
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;

	float3x3 LtV = LocalToView(input.rotScale, input.pos);

	output.Position = float4(mul(LtV, float3(input.pos, 1)).xy, 0, 0);
	output.Color = input.color;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return input.Color;
}

technique Unlit
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};