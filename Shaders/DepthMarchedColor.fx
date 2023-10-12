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

float4 ObjRSS;
float2 ObjT;

float4 Color;

float ClipOffset;

struct VertexShaderInput
{
	float4 Position : POSITION;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float DepthValue : TEXCOORD0;
	float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;

	float3x3 LtV = LocalToView(ObjRSS, ObjT);

	output.Position = float4(mul(LtV, float3(input.Position.xy, 1.0f)).xy, input.Position.z, 1.0f);
	output.Color = Color;
    output.DepthValue = input.Position.z;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    //clip(input.DepthValue + ClipOffset);
	return float4(0, 0, 0, 1.0f);
}

technique Unlit
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};