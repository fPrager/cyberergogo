//------------------------------------------------------
//--                                                  --
//--		   www.riemers.net                    --
//--   		    Basic shaders                     --
//--		Use/modify as you like                --
//--                                                  --
//------------------------------------------------------

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

//------- Constants --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;


//------- Technique: StreetFromAbove --------

//special constants

float xAreaWidth;
float xAreaHeight;

VertexToPixel StreetFromAboveVS( float4 inPos : POSITION, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;

	float halfAreaWidth = xAreaWidth/2;
	float halfAreaHeight = xAreaHeight/2;
	float newX = (abs(inPos.x) - halfAreaWidth)/halfAreaWidth;
	float newY = (abs(inPos.z) - halfAreaHeight)/halfAreaHeight;
	float4 newPos = float4(newX,newY,0,inPos.w);
	Output.Position = newPos;
	Output.Color = inColor;

	return Output;    
}

PixelToFrame StreetFromAbovePS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	Output.Color = PSIn.Color;

	return Output;
}

technique StreetFromAbove
{
	pass Pass0
	{   
		VertexShader = compile vs_2_0 StreetFromAboveVS();
		PixelShader  = compile ps_2_0 StreetFromAbovePS();
	}
}



sampler TextureSampler : register(s0);

#define SAMPLE_COUNT 15

float4x4 MatrixTransform : register(vs, c0);
float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];


// Technique: Fading

void FadingVS(inout float4 color    : COLOR0, 
                        inout float2 texCoord : TEXCOORD0, 
                        inout float4 position : SV_Position) 
{ 
    position = mul(position, MatrixTransform); 
} 

float4 FadingPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 org_c = tex2D(TextureSampler, texCoord);
	float4 c = org_c;

	float nHeight = org_c.r;
	int nField = -1;
	for(int i = SAMPLE_COUNT-1; i>=0; i--)
	{
			float4 ct = tex2D(TextureSampler, texCoord + SampleOffsets[i]);
			if(ct.b == 1)
			{
				nField = i;
				nHeight = ct.r;
			}

	}
	if(nField!=-1)
	{
		c.r = max(c.r,nHeight*SampleWeights[nField]);
		c.g = max(c.g,SampleWeights[nField]);
	}    
    return c;
}


technique Fading
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 FadingVS(); 
        PixelShader = compile ps_3_0 FadingPS();
    }
}

//Technique: Combine

sampler Color1Sampler : register(s0);
sampler Color2Sampler : register(s1);


void CombineVS(inout float4 color    : COLOR0, 
                        inout float2 texCoord : TEXCOORD0, 
                        inout float4 position : SV_Position) 
{ 
    position = mul(position, MatrixTransform); 
} 


float4 CombinePS(float2 texCoord : TEXCOORD0) : COLOR0
{

	float4 c1 = tex2D(Color1Sampler, texCoord);
	float4 c2 = tex2D(Color2Sampler, texCoord);
	if(c1.g > c2.g)
		return c1;
	else
		return c2;
}

technique Combine
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 CombineVS();
        PixelShader = compile ps_3_0 CombinePS();
    }
}

//Technique: Bluring

void BluringVS(inout float4 color    : COLOR0, 
                        inout float2 texCoord : TEXCOORD0, 
                        inout float4 position : SV_Position) 
{ 
    position = mul(position, MatrixTransform); 
} 

float4 BluringPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 c = tex2D(TextureSampler, texCoord);
    
    // Combine a number of weighted image filter taps.
	bool toReset = false;
    if(c.g==0)
	{
	c.g = 1;
	c.r = 0;
	c.b = 1;
	c.a = 1;
	toReset = true;
	for (int i = 0; i < SAMPLE_COUNT; i++)
    {
		float4 ct = tex2D(TextureSampler, texCoord + SampleOffsets[i]);
		if(ct.g > 0 && ct.g * SampleWeights[i] < c.g)
        c.r += 0.1;
    }
	}
	if(toReset && c.g==1)
	c.g = 0;
	//if(c.g==0)
	//	c=float4(1,0,0,1);
    return c;
}


technique Bluring
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 BluringVS();
        PixelShader = compile ps_3_0 BluringPS();
    }
}