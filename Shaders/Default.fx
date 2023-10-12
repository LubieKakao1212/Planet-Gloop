#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#include "Transforms.fxh"
#include "Camera.fxh"

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	//float2 WPos : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input,
	float4 rotScale : POSITION1,
	float2 pos : POSITION2,
	float4 color : COLOR0)
{
	VertexShaderOutput output;

	float3x3 LtV = LocalToView(rotScale, pos);

	//output.WPos = input.Position.xy;

	output.Position = float4(mul(LtV, float3(input.Position.xy, 1.0f)).xy, 0.0f, 1.0f);
	output.Color = color;

	//mul(LtV, float3(input.Position.xy, 1.0f)).xy;

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