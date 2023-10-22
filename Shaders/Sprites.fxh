
int AtlasSize;

Texture3D SpriteAtlas : register(t0);

SamplerState AtlasSampler : register(s0);
// {
//     Texture = (SpriteAtlas);
//     Filter = POINT;
//     AddressU = Wrap;
//     AddressV = Wrap;
// };

float3 ProcessSpritePos(float4 atlasPos, float2 UV) {
    float2 spriteSize = atlasPos.zw;
	float2 spritePos = float2(frac(atlasPos.x), atlasPos.y);
	float atlasIdx = floor(atlasPos.x);

	return float3(spritePos + (UV * spriteSize), atlasIdx / AtlasSize);
}