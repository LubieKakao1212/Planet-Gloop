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
#include "Sprites.fxh"

float4 GridRS;
float2 GridT;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct TileTransformInput
{
	//Rotation Scale Skew
	float4 RSS : POSITION3;
	//Translation
	float2 T : POSITION4;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	//float2 WorldPosition : POSITION1;
	float2 UV : TEXCOORD0;
	float3 AtlasPos : TEXCOORD1;
};

VertexShaderOutput MainVS(
	in VertexShaderInput input,
	in TileTransformInput tileInput,
	float4 rotScale : POSITION1,
	float2 pos : POSITION2,
	
	float4 color : COLOR0,
    float4 atlasPos : TEXCOORD1)
{
	VertexShaderOutput output;

    float3x3 GridToWorld = ComposeTransform(GridRS, GridT);
	//float3x3 TileToGrid = ComposeTransform(tileInput.RSS, tileInput.T);
    float3x3 LocalToTile = ComposeTransform(rotScale, float2(0,0));
    //float3x3 Translation = ComposeTransform(float4(1,0,0,1), pos);

    float3x3 LtG = LocalToTile;//mul(TileToGrid, LocalToTile);

    float3x3 GtV = mul(Projection(), GridToWorld);

	//output.WPos = input.Position.xy;

	float3 position = mul(LtG, float3(input.Position.xy, 1.0f));
	position.xy += pos;

	output.Position = float4(mul(GtV, position).xy, 0.0f, 1.0f);

	output.Color = color;

	output.AtlasPos = ProcessSpritePos(atlasPos, input.UV);

	output.UV = input.UV;


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