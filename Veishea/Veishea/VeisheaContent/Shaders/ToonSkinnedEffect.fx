#include "Skinning.fxh"

float3 lightPositions[10];

// The texture that contains the celmap
texture CelMap;
sampler2D CelMapSampler = sampler_state
{
	Texture	  = <CelMap>;
	MIPFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
};

struct ToonVSOutput
{
    float2 TexCoord   : TEXCOORD0;
    float4 PositionPS : SV_Position;
	float3 normal     : TEXCOORD1;
	float3 worldPos   : TEXCOORD2;
};

// Vertex shader: vertex lighting, four bones.
ToonVSOutput VSToon(VSInputNmTxWeights vin)
{
	Skin(vin, 4);

    ToonVSOutput output;
    
    output.TexCoord = vin.TexCoord;
    output.PositionPS = mul(vin.Position, WorldViewProj);

	output.worldPos = mul(vin.Position, World);
	output.normal = mul(vin.Normal, World);
    
    return output;
}

// Pixel shader: vertex lighting.
float4 PSToonPointLight(ToonVSOutput pin) : SV_Target0
{
	float4 Color = SAMPLE_TEXTURE(Texture, pin.TexCoord);
	
	float light = 0;

	float3 lightDir;
	float amt;
	float att;
	float tmp;

	for(int i=0; i<6; ++i)
	{
		lightDir = normalize(lightPositions[i] - pin.worldPos);
		amt = saturate(dot(pin.normal, lightDir));
		att = saturate(distance(lightPositions[i], pin.worldPos) / 70);
		att = 1 - att;
		tmp = amt * att;
		light += tmp;
	}

    Color.rgb *= light + float3(.25, .25, .25);

    return Color;
}

Technique Toon
{
    Pass
    {
        VertexShader = compile vs_2_0 VSToon();
        PixelShader  = compile ps_2_0 PSToonPointLight();
    }
}