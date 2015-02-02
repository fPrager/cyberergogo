struct VertexToPixel
{
    float4 Position   	: POSITION;    
    float4 Color		: COLOR0;
    float2 TextureCoords: TEXCOORD1;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};

//SimpleQuadShader

sampler QuadTextureSampler : register(s0);


float4 SimpleQuadShaderPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 quadColor = tex2D(QuadTextureSampler, texCoord);
	return quadColor;
}


technique SimpleQuadShader
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 SimpleQuadShaderPS();
    }
}
