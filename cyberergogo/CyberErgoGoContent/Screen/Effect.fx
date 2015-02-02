struct VertexToPixel
{
    float4 Position   	: POSITION;    
    float4 Color		: COLOR0;
	float LightingFactor: TEXCOORD0;
    float2 TextureCoords: TEXCOORD1;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};

//------Constants ---------------

//----- general constants ------------------

			//unchangeging constants ----------- (doesn't change every drawing)

float3 xLightDirection;
float xAmbient;


			//changing constants --------------- (the values are diffrent in every draw-cycle)

float4x4 xViewMatrix;
float4x4 xProjectionMatrix;
float4x4 xWorldMatrix;
float3 xCamPos;
float3 xCamUp;

float xXPosition;
float xYPosition;
float xZPosition;

float xOpacity;

Texture xCanvasTexture;

sampler CanvasTextureSampler = sampler_state { texture = <xCanvasTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = clamp; AddressV = clamp;};

VertexToPixel CanvasShadingVS( float4 inPos : POSITION, float2 inTexCoords: TEXCOORD0)
{
     VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);

	inPos.x += xXPosition;
	inPos.y += xYPosition;
	inPos.z += xZPosition;

	Output.Position = mul(inPos, preWorldViewProjection);	
    Output.TextureCoords = inTexCoords;
    return Output;
}

PixelToFrame CanvasShadingPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color =  tex2D(CanvasTextureSampler, PSIn.TextureCoords);
	Output.Color = float4(1,1,0,1);
	if(Output.Color.a > 0)
		Output.Color.a = xOpacity;
	
    return Output;
}

technique CanvasShading
{
	pass Pass0
	{     
		VertexShader = compile vs_2_0 CanvasShadingVS();
		PixelShader  = compile ps_2_0 CanvasShadingPS();
	}
}


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
	AlphaBlendEnable  = true;
        PixelShader = compile ps_2_0 SimpleQuadShaderPS();
    }
}
