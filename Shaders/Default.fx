#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

#include "Transforms.fxh"
#include "Camera.fxh"

Texture2DArray SpriteAtlas;

SamplerState AtlasSampler
{
    Filter = POINT;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
	float3 AtlasPos : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input,
	float4 rotScale : POSITION1,
	float2 pos : POSITION2,
	float4 color : COLOR0,
	float4 atlasPos : TEXCOORD1)
{
	VertexShaderOutput output;

	float3x3 LtV = LocalToView(rotScale, pos);

	//output.WPos = input.Position.xy;

	output.Position = float4(mul(LtV, float3(input.Position.xy, 1.0f)).xy, 0.0f, 1.0f);
	output.Color = color;

	float2 spriteSize = atlasPos.zw;
	float2 spritePos = float2(frac(atlasPos.x), atlasPos.y);
	float atlasIdx = floor(atlasPos.x);

	output.AtlasPos = float3(spritePos + (input.UV * spriteSize), atlasIdx);

	output.UV = input.UV;

	//mul(LtV, float3(input.Position.xy, 1.0f)).xy;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return SpriteAtlas.SampleLevel(AtlasSampler, input.AtlasPos, 0) * input.Color;
}

technique Unlit
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};