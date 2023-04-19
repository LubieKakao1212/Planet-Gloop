#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

struct VertexShaderInput
{
	float4 Position : POSITION;
	float4 rotScale : TEXCOORD0;
	float4 color : COLOR0;
};

struct SimpleVSInput
{
	float3 Position : POSITION;
};

struct SimpleVSOutput
{
	float4 Position : SV_POSITION;
	float4 Pos1 : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

SimpleVSOutput SimpleVS(in SimpleVSInput input)
{
	SimpleVSOutput output;

	output.Position = float4(input.Position, 1);
	output.Pos1 = float4(input.Position, 1);

	return output;
}

float4 SimplePS(SimpleVSOutput input) : COLOR
{
	return float4(1,1,1,1);
}

VertexShaderOutput MainVS(in VertexShaderInput input, float2 pos : TEXCOORD1)
{
	VertexShaderOutput output;

	float4 rotScale = input.rotScale;

	float2x2 rs = float2x2(
		rotScale.x, rotScale.y, 
		rotScale.z, rotScale.w);

	output.Position = float4(mul(rs, input.Position.xy) + pos, 0.f, 1.f);
	//output.Position = input.Position + float4(pos, 0.f, 1.f);
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

technique Simple
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL SimpleVS();
		PixelShader = compile PS_SHADERMODEL SimplePS();
	}
}