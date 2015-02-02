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

//StreetOnTerrainMapping

Texture xTerrainTexture;
Texture xStreetTexture;
Texture xBaseTexture;

sampler TerrainSampler = sampler_state{
			texture = <xTerrainTexture>;
			MipFilter = LINEAR;
			MinFilter = LINEAR;
			MagFilter = LINEAR;
			AddressU  = Wrap;
			AddressV  = Wrap;
};
sampler StreetSampler = sampler_state{
			texture = <xStreetTexture>;
			MipFilter = LINEAR;
			MinFilter = LINEAR;
			MagFilter = LINEAR;
			AddressU  = Wrap;
			AddressV  = Wrap;
};
sampler BaseSampler = sampler_state{
			texture = <xBaseTexture>;
			MipFilter = LINEAR;
			MinFilter = LINEAR;
			MagFilter = LINEAR;
			AddressU  = Wrap;
			AddressV  = Wrap;
};


float4 CombineColors(float4 terrain, float4 street)
{
	//Aufbau des street-Farbvektors:
	// street.r = Höhenangabe (evtl. nur anteilig, wegen Verwischungseffekt)
	// street.g = Stellt den Anteil der Höhenangabe dar, in wie fern sie der originalen Straßenhöhe entspricht (0.5 = die Höhenangabe in street.r ist die Hälfte der Originalhöhe)
	// street.b = wenn 1 = Straßenkoordinate, wenn 0 = Terrainkoordinate

	if(street.b == 1) 
		return float4(street.r, 1, 0, 1);

	if(street.g == 0)
		return float4(terrain.r,terrain.r,terrain.r,1);

	float height = terrain.r*(1-street.g) + street.r;
	//float height = 1/street.g * street.r;

	return float4(height,height,height,1);
}

float4 StreetOnTerrainMappingPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 street = tex2D(StreetSampler, float2(texCoord.x, 1-texCoord.y));
	float4 terrain = tex2D(TerrainSampler, texCoord);
	return CombineColors(terrain, street);
}


technique StreetOnTerrainMapping
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 StreetOnTerrainMappingPS();
    }
}


//StreetOnTerrainMappingWithSmoothing
float Border = 0.2;

float4 SmoothTerrainLikeAnIsland(float4 terrain, float2 texCoord)
{
	float distanceFromTheMiddle = 0;
	float colorValue = 1;
	distanceFromTheMiddle = length(float2(0.5,0.5) - texCoord);
	if(distanceFromTheMiddle > 0.5 - Border && distanceFromTheMiddle < 0.5)
		{
			colorValue = (0.5 - distanceFromTheMiddle) / Border;
			colorValue = colorValue * colorValue;
		}
	
	if(distanceFromTheMiddle >= 0.5)
			colorValue = 0;
	return float4(terrain.r * colorValue,terrain.g,terrain.b,1);
}

float4 StreetOnTerrainMappingWithSmoothingPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 street = tex2D(StreetSampler, float2(texCoord.x, 1-texCoord.y));
	float4 terrain = tex2D(TerrainSampler, texCoord);
	terrain = SmoothTerrainLikeAnIsland(terrain, texCoord);
	return CombineColors(terrain, street);
}


technique StreetOnTerrainMappingWithSmoothing
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 StreetOnTerrainMappingWithSmoothingPS();
    }
}



//StreetOnTerrainMappingWithSmoothingAndBase

float baseBorder = 50/300;

VertexToPixel TerrainMappingWithSmoothingAndBaseVS( float4 inPos : POSITION, float4 inColor: COLOR, float2 inTex:TEXCOORD)
{	
	VertexToPixel Output = (VertexToPixel)0;
	
	Output.Position = inPos;
	Output.Color = inColor;
	Output.TextureCoords = inTex;
    
	return Output;    
}

PixelToFrame TerrainMappingWithSmoothingAndBasePS(VertexToPixel PSIn) : COLOR0
{
	float4 base = tex2D(BaseSampler, PSIn.TextureCoords);
	float4 street = tex2D(StreetSampler, float2(PSIn.TextureCoords.x, PSIn.TextureCoords.y));
	
	if((PSIn.TextureCoords.x > baseBorder && PSIn.TextureCoords.x < 1-baseBorder) && (PSIn.TextureCoords.y > baseBorder && PSIn.TextureCoords.y < 1-baseBorder))
	  {
	  	float2 innerTexCoord = (PSIn.TextureCoords-baseBorder)/(1-(2*baseBorder));
		float4 terrain = tex2D(TerrainSampler, innerTexCoord);
	  	terrain = SmoothTerrainLikeAnIsland(terrain, innerTexCoord);
	  	float combinedHeight = base.r + (1-base.r) * terrain.r;
	  	base = float4(combinedHeight, combinedHeight,combinedHeight,1);
	  //	base = terrain;
	}
	//base = CombineColors(base, street);

	PixelToFrame Output = (PixelToFrame)0;		
	Output.Color = base;
	return Output;
}


technique TerrainMappingWithSmoothingAndBase
{
    pass Pass1
    {
	VertexShader = compile vs_3_0 TerrainMappingWithSmoothingAndBaseVS();
        PixelShader = compile ps_3_0 TerrainMappingWithSmoothingAndBasePS();
    }
}