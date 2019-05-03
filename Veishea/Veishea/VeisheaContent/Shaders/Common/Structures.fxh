#ifndef STRUCTURES_FXH
#define STRUCTURES_FXH

//////////////////////////////////////////////
// Vertex Shader Input
//////////////////////////////////////////////

struct GBufferVSInput_Default
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct GBufferVSInput_Common
{
	float4 Position : POSITION0;
	float3 Normal   : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct GBufferVSInput_Bi_Ta
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	float3 Binormal : BINORMAL0;
	float3 Tangent : TANGENT0;
};

struct GBufferVSInput_Blend
{
	float4 Position : POSITION0;
	float3 Normal   : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	int4 BlendIndices : BLENDINDICES0;
	float4 BlendWeights : BLENDWEIGHT0;
};

struct GBufferVSInput_Bi_Ta_Blend
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	int4 BlendIndices : BLENDINDICES0;
	float4 BlendWeights : BLENDWEIGHT0;
};

//////////////////////////////////////////////
// Vertex Shader Output
//////////////////////////////////////////////

struct GBufferVSOutput_Default
{
	float4 Position : POSITION0;
	float3 NormalWS : TEXCOORD0;
	float2 Depth : TEXCOORD1;
};

struct GBufferVSOutput_Common
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 NormalWS   : TEXCOORD1;
	float2 Depth    : TEXCOORD2;
};

struct GBufferVSOutput_Normal
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float2 Depth : TEXCOORD1;
	float3x3 TangentToWorld : TEXCOORD3;
};

struct GBufferVSOutput_NormalParallax
{
	float4 Position      		: POSITION0;
	float3 NormalWS    			: NORMAL0;
	float2 TexCoord 			: TEXCOORD0;
	float3 ViewTS   			: TEXCOORD1; // view vector in tangent space, denormalized
	float2 ParallaxOffsetTS 	: TEXCOORD2; // Parallax offset vector in tangent space
	float3 ViewWS 				: TEXCOORD3;
	float2 Depth 				: TEXCOORD4;
	float3x3 TangentToWorld 	: TEXCOORD5; // Matrix to transform from tangent space to world space
};

//////////////////////////////////////////////
// Pixel Shader Output
//////////////////////////////////////////////
struct GBufferPSOutput
{
	float4 Depth : COLOR0;
	float4 Albedo : COLOR1;
	float4 Highlights : COLOR2;
	float4 Normal : COLOR3;
};

#endif