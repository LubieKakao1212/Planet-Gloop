#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

#include "Constants.fxh"
#include "Transforms.fxh"
#include "Camera.fxh"
#include "Sprites.fxh"

//distance, radial
float2 Falloffs;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;

	float2 OPos : TEXCOORD3;
	
    float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
	float3 AtlasPos : TEXCOORD1;
    //distance, thickness, direciton, angle
	float4 SegmentSize : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input,
	float4 rotScale : POSITION1,
	float2 pos : POSITION2,
	float4 color : COLOR1,
	float4 atlasPos : TEXCOORD1,
    float4 segmentSize : TEXCOORD2)
{
	VertexShaderOutput output;

    float3x3 proj = Projection();
	float3x3 LtW = ComposeTransform(rotScale, pos);
    float3x3 LtV = mul(proj, LtW);

    output.OPos = mul(LtW, float3(input.Position.xy, 0.0f)).xy;

	output.Position = float4(mul(LtV, float3(input.Position.xy, 1.0f)).xy, 0.0f, 1.0f);
	output.Color = color;

	output.AtlasPos = ProcessSpritePos(atlasPos, input.UV);

	output.UV = input.UV;

    output.SegmentSize = segmentSize;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 SS = input.SegmentSize;
    float distance = length(input.OPos);
    float angle = atan2(input.OPos.y, input.OPos.x);

    float d = clamp(-1.0f/Falloffs.x * (abs(distance - SS.x) - SS.y), 0.0f, 1.0f);
    float a = clamp(-1.0f/Falloffs.y * (abs(fmod(angle, PI * 2) - SS.z) - SS.w), 0.0f, 1.0f);
    //float a1 = clamp(-1.0f/Falloffs.y * (abs(angle - SS.z) - SS.w), 0.0f, 1.0f);
    
    //float a = max(a0, a1);

	return (a * d) /* *SpriteAtlas.SampleLevel(AtlasSampler, input.AtlasPos, 0)*/ * input.Color;
}

technique Unlit
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};