#include "../Common/NormalEncoding.fxh"

float3 Color = float3(1,1,1);
float3 CameraPosition;

texture ColorMap;
texture NormalMap;
texture DepthMap;
texture HighlightMap;

float2 halfPixel = float2(0,0);

float4x4 InverseViewProjection;

sampler colorSampler
{
	Texture = ColorMap;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	MipFilter = POINT;
};

sampler depthSampler
{
	Texture = DepthMap;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	MipFilter = POINT;
};

sampler normalSampler
{
	Texture = NormalMap;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	MipFilter = POINT;
};

sampler highlightSampler
{
	Texture = HighlightMap;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	MipFilter= POINT;

};

struct LightVertexShaderInput
{
    float3 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct LightVertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

float3 GetNormal(float2 texCoord)
{
	float2 encodedNormals = tex2D(normalSampler,texCoord).xy;
	return decodeNormals(encodedNormals);
}

float3 GetNormalWithClip(float2 texCoord, out int invalid)
{
	float2 encodedNormals = tex2D(normalSampler,texCoord).xy;
	invalid = encodedNormals == 0 ? 0 : 1;
	return decodeNormals(encodedNormals);
}

float4 GetFragmentWorldPosition(float2 texCoord)
{
	float depthValue = tex2D(depthSampler, texCoord).r;

	//Calculate world position
	float4 position;
	position.x = 2 * texCoord.x - 1.0f;
	position.y = -2 * texCoord.y + 1.0f;
	position.z = depthValue;
	position.w = 1.0f;

	position = mul(position,InverseViewProjection);
	position /= position.w;

	return position;
}

LightVertexShaderOutput LightVertexShaderFunction(LightVertexShaderInput input)
{
    LightVertexShaderOutput output;

	output.Position = float4(input.Position,1);
	output.TexCoord = input.TexCoord + halfPixel;

    return output;
}

