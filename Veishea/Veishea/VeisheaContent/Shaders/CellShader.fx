float3 lightPositions[10];
float4x4 World;
float4x4 ViewProj;
float4x4 InverseWorld;

texture ColorMap;
sampler ColorMapSampler = sampler_state
{
   Texture = <ColorMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;
   AddressU  = Wrap;
   AddressV  = Wrap;
};

struct ToonVSOutput
{
    float2 TexCoord   : TEXCOORD0;
    float4 PositionPS : SV_Position;
	float3 normal     : TEXCOORD1;
	float3 worldPos   : TEXCOORD2;
};

ToonVSOutput ToonVS( float4 Pos: POSITION, float2 Tex : TEXCOORD, float3 N: NORMAL)
{

    ToonVSOutput output;
    
    output.TexCoord = Tex;
    output.PositionPS = mul(mul(Pos, World), ViewProj);

	output.worldPos = mul(Pos, World);
	output.normal = mul(N, World);
    
    return output;
}

// Pixel shader: vertex lighting.
float4 ToonPS(ToonVSOutput pin) : COLOR
{
	float4 Color = tex2D(ColorMapSampler, pin.TexCoord);
	
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

	light += float3(.25, .25, .25);

    Color.rgb *= light;
    return Color;
}

technique Toon
{
	pass P0
	{
		
		VertexShader = compile vs_2_0 ToonVS();
		PixelShader = compile ps_2_0 ToonPS();
	}
}