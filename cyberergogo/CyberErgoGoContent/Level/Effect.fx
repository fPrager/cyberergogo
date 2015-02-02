//This effect file stores the representation of a level with its six components (skybox, terrain, street, terrainobejects, gameobjects and moving object)

struct VertexToPixel
{
    float4 Position   	: POSITION;    
    float4 Color		: COLOR0;
    float LightingFactor: TEXCOORD0;
    float2 TextureCoords: TEXCOORD1;
	float LightingFactor2: TEXCOORD3;
	float3 Normal: TEXCOORD4;
	float Height : TEXCOORD2;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};


//----- general constants ------------------

			//unchangeging constants ----------- (doesn't change every drawing)

float3 xLightDirection;
float xAmbient;
Texture xSimplePoint;

sampler SimplePointSampler = sampler_state { texture = <xSimplePoint> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};


			//changing constants --------------- (the values are diffrent in every draw-cycle)

float4x4 xViewMatrix;
float4x4 xProjectionMatrix;
float4x4 xWorldMatrix;
float4x4 xInvertedViewMatrix;
float3 xCameraPosition;
float3 xCameraUp;


//---- Step1: SkyBox	---------------------------------------------------------------------------------------------------------------------------------------------------	Skybox

			//unchanging constants

			//changing constants


VertexToPixel SkyBoxVS(float4 inPos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;

	return Output;
}

PixelToFrame SkyBoxPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;	

	return Output;
}

technique SkyBoxShading
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 SkyBoxVS();
		PixelShader  = compile ps_2_0 SkyBoxPS();
	}
}

//---- Step2: Terrain	--------------------------------------------------------------------------------------------------------------------------------------------------	Terrain

//static shading

			//unchanging constants

			float FirstBorder = 0.2;
			float SecondBorder = 0.8;
			
			//changing constants

			float TerrainMaxHeight;
			float TerrainMinHeight;
			
			Texture xHeightMapWithStreet;
			sampler HeightMapSampler = sampler_state { texture = <xHeightMapWithStreet> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};
			

			Texture xLevel0Texture;
			Texture xLevel1Texture;
			Texture xLevel2Texture;
			Texture xLevel3Texture;
			Texture xStreetTexture;
			float xLevel0to1;
			float xLevel1to2;
			float xLevel2to3;

			sampler Level0Sampler = sampler_state{
			texture = <xLevel0Texture>;
};
sampler Level1Sampler = sampler_state{
			texture = <xLevel1Texture>;
};
sampler Level2Sampler = sampler_state{
			texture = <xLevel2Texture>;
			AddressV  = Wrap;
};
sampler Level3Sampler = sampler_state{
			texture = <xLevel3Texture>;
};
sampler StreetSampler = sampler_state{
			texture = <xStreetTexture>;
};


VertexToPixel StaticTerrainVS(float4 pos: POSITION0, float3 normal: NORMAL, float2 tex :TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;

	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);

	
	Output.Color = float4(1,1,0,1);
	
	//float4 heightColor = tex2D(HeightMapSampler, tex);
	//float h = 0;
	//if(heightColor.g == 1)
	//{
	//	float4 streetColor = tex2D(StreetSampler, tex);
	//	h = (0.5 - streetColor.r)*2;
	//}
	
	Output.Position = mul(pos, preWorldViewProjection);
	Output.Height = pos.y;
	Output.TextureCoords = tex;

	float4 Normal = normalize(mul(float4(normalize(normal),0),xWorldMatrix));	
	Output.Normal = float3(Normal.x,Normal.y,Normal.z);
	return Output;
}

PixelToFrame StaticTerrainPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	float4 level0 = tex2D(Level0Sampler, PSIn.TextureCoords*2);
	float4 level1 = tex2D(Level1Sampler, PSIn.TextureCoords*2);
	float4 level2 = tex2D(Level2Sampler, PSIn.TextureCoords*2);
	float4 level3 = tex2D(Level3Sampler, PSIn.TextureCoords*2);
	float4 street = tex2D(StreetSampler, PSIn.TextureCoords*50);
	
	float4 LevelWeight;

	float4 terrainColor = tex2D(HeightMapSampler, PSIn.TextureCoords);
	float4 color = street;
	if(terrainColor.g != 1)
	{
	float colorHeight = terrainColor.r;
	LevelWeight[0] = clamp(1.0f - abs(colorHeight - 0)/0.1, 0, 1);
	LevelWeight[1] = clamp(1.0f - abs(colorHeight - xLevel0to1)/0.3, 0, 1);
	LevelWeight[2] = clamp(1.0f - abs(colorHeight - xLevel1to2)/0.3, 0, 1);
	LevelWeight[3] = clamp(1.0f - abs(colorHeight - xLevel2to3)/0.1, 0, 1);

	float total = LevelWeight[0] + LevelWeight[1] + LevelWeight[2] + LevelWeight[3];
	LevelWeight[0] /= total;
	LevelWeight[1] /= total;
	LevelWeight[2] /= total;
	LevelWeight[3] /= total;
	color = LevelWeight[0] * level0 + LevelWeight[1] * level1  + LevelWeight[2] * level2  + LevelWeight[3] * level3;
	}

	//cell shading
	float intensity = dot(normalize(-xLightDirection), PSIn.Normal);

	if(intensity < 0)
        intensity = 0;

	if (intensity > 0.9)
        color = float4(1,1,1,1.0) * color;
    else if (intensity > 0.5)
        color = float4(0.7,0.7,0.7,1.0) * color;
	else if (intensity > 0.3)
        color = float4(0.4,0.4,0.4,1.0) * color;
    else 
        color = float4(0.2,0.2,0.2,1.0) * color;
	Output.Color = color;
	
	return Output;
}

VertexToPixel OutlineStaticTerrainVS(float4 pos: POSITION0, float3 normal: NORMAL, float2 tex :TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);

	float4 mulNormal = normalize(mul(float4(normalize(normal),0),preWorldViewProjection));	
	float4 originalPosition = mul(pos, preWorldViewProjection); 

	Output.Position = originalPosition + (mul(0.5, mulNormal)); 

	return Output;    
}
 
PixelToFrame OutlineStaticTerrainPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	Output.Color = float4(0,0,0,1);
    return Output;
}

technique StaticTerrainShading
{
	pass Pass0
	{
		ZENABLE = TRUE;
        ZWRITEENABLE = TRUE;
        CULLMODE = CW;
		VertexShader = compile vs_3_0 OutlineStaticTerrainVS();
		PixelShader  = compile ps_3_0 OutlineStaticTerrainPS();
	}

	pass Pass1
	{
		ZENABLE = TRUE;
        ZWRITEENABLE = TRUE;
        CULLMODE = CCW;
		VertexShader = compile vs_3_0 StaticTerrainVS();
		PixelShader  = compile ps_3_0 StaticTerrainPS();
	}
}

//---- Step3: Street	--------------------------------------------------------------------------------------------------------------------------------------------------	Street

			//unchanging constants

			//changing constants


VertexToPixel StreetVS(float4 pos: POSITION0, float3 normal: NORMAL)
{
	VertexToPixel Output = (VertexToPixel)0;

	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);

	Output.Position = mul(pos, preWorldViewProjection);
	Output.Color = float4(0.5,0.5,0.5,1);

	float3 n = normalize(mul(normalize(normal), xWorldMatrix));	
	Output.LightingFactor = 1;
	
	Output.LightingFactor = dot(n, xLightDirection);

	return Output;
}

PixelToFrame StreetPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	Output.Color = PSIn.Color;
	Output.Color.rgb *= saturate(PSIn.LightingFactor) + xAmbient;

	return Output;
}

technique StreetShading
{
	pass Pass0
	{
	//AlphaBlendEnable = True;
    //SrcBlend = SrcAlpha;
    //DestBlend = InvSrcAlpha; 
		VertexShader = compile vs_2_0 StreetVS();
		PixelShader  = compile ps_2_0 StreetPS();
	}
}

//---- Step4: Skydome	----------------------------------------------------------------------------------------------------------------------------------------------	Skydome

//unchanging constants
		float CloudTextureRepeat = 5;	
		float4 SkyHorizonColor = float4(1,1,1,1);

//changing constants
		Texture xSkyboxTexture;
		sampler SkyboxSampler = sampler_state { texture = <xSkyboxTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};
		
		Texture xSkyboxGradientTexture;
		sampler SkyboxGradientSampler = sampler_state { texture = <xSkyboxGradientTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};	
		float xCloudMoving;

VertexToPixel SkydomeShadingVS(float4 pos: POSITION0, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);
    
	Output.Position = mul(pos, preWorldViewProjection);	
	Output.TextureCoords = inTexCoords;
	
	float3 Normal = normalize(mul(normalize(inNormal), xWorldMatrix));	
    
	return Output;     
}

PixelToFrame SkydomeShadingPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;		
    float2 cloudTexCoord = float2(PSIn.TextureCoords.x + xCloudMoving, PSIn.TextureCoords.y)* CloudTextureRepeat;
	float4 cloudColor  = tex2D(SkyboxSampler, cloudTexCoord);
	float gradientValue  = tex2D(SkyboxGradientSampler, PSIn.TextureCoords).a;
	
	Output.Color = cloudColor * gradientValue + SkyHorizonColor *(1-gradientValue);
	return Output;
}

technique SkydomeShading
{
	pass Pass0
	{
		ZENABLE = TRUE;
        ZWRITEENABLE = FALSE;
        CULLMODE = NONE;
		VertexShader = compile vs_2_0 SkydomeShadingVS();
		PixelShader  = compile ps_2_0 SkydomeShadingPS();
	}
}

//---- Step5: Watershading	----------------------------------------------------------------------------------------------------------------------------------------------	WaterShading

//unchanging constants
	float SkyMirageFactor = 0.3;
	

//changing constants
	float xTideHeight;
	float xTideOffset;
			
float4 GetSkyMirage(float2 texCoord, float4 waterColor)
{
	float2 cloudTexCoord = float2(texCoord.x + xCloudMoving, texCoord.y)* CloudTextureRepeat;
	float4 skyColor = tex2D(SkyboxSampler, cloudTexCoord);
	
	if(skyColor.r*255 >= 200)
	skyColor += float4(0.1,0.1,0.1,0);
	
	return skyColor*SkyMirageFactor + waterColor * (1-SkyMirageFactor);
}


VertexToPixel WaterShadingVS(float4 pos: POSITION0, float4 color: COLOR, float2 tex: TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;
	
	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);
    Output.TextureCoords = tex;
	pos.y += xTideHeight * xTideOffset;
	Output.Position = mul(pos, preWorldViewProjection);
	Output.Color = color;
	return Output;
}

PixelToFrame WaterShadingPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;	
	
	Output.Color =  GetSkyMirage(PSIn.TextureCoords, PSIn.Color);
	return Output;
}

technique WaterShading
{
	pass Pass0
	{
		ZENABLE = TRUE;
        ZWRITEENABLE = TRUE;
        CULLMODE = NONE;
		VertexShader = compile vs_2_0 WaterShadingVS();
		PixelShader  = compile ps_2_0 WaterShadingPS();
	}
}


//---- Step4: TerrainObject	----------------------------------------------------------------------------------------------------------------------------------------------	TerrainObjects

			//unchanging constants

			//changing constants


VertexToPixel TerrainObjectVS(float4 inPos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;

	return Output;
}

PixelToFrame TerrainObjectPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;	

	return Output;
}

technique TerrainObjectShading
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 TerrainObjectVS();
		PixelShader  = compile ps_2_0 TerrainObjectPS();
	}
}

//---- Step5: GameObject	----------------------------------------------------------------------------------------------------------------------------------------------	GameObject

			//unchanging constants

			//changing constants


VertexToPixel GameObjectVS(float4 inPos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;

	return Output;
}

PixelToFrame GameObjectPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;	

	return Output;
}

VertexToPixel OutlineGameObjectVS(float4 pos: POSITION0, float3 normal: NORMAL, float2 tex :TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);

	float4 mulNormal = normalize(mul(float4(normalize(normal),0),xWorldMatrix));	
	float4 originalPosition = mul(pos, preWorldViewProjection); 

	Output.Position = originalPosition + (mul(0.3, mulNormal)); 

	return Output;    
}
 
PixelToFrame OutlineGameObjectPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	Output.Color = float4(0,0,0,1);
    return Output;
}

technique GameObjectShading
{

	pass Pass0
	{
		ZENABLE = TRUE;
        ZWRITEENABLE = TRUE;
        CULLMODE = CW;
		VertexShader = compile vs_3_0 OutlineGameObjectVS();
		PixelShader  = compile ps_3_0 OutlineGameObjectPS();
	}

	pass Pass1
	{
		ZENABLE = TRUE;
        ZWRITEENABLE = TRUE;
        CULLMODE = CCW;
		VertexShader = compile vs_2_0 GameObjectVS();
		PixelShader  = compile ps_2_0 GameObjectPS();
	}

}

//---- Step6: MovingObject	----------------------------------------------------------------------------------------------------------------------------------------------	MovingObjects

			//unchanging constants

			//changing constants
float3 xObjectColor;

VertexToPixel MovingObjectVS(float4 pos : POSITION, float3 normal: NORMAL)
{
	VertexToPixel Output = (VertexToPixel)0;

	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);
    
	Output.Position = mul(pos, preWorldViewProjection);
	//Output.Color = color;
	Output.Color = float4(xObjectColor.x, xObjectColor.y, xObjectColor.z, 1);
	float3 Normal = normalize(mul(normalize(normal), preWorldViewProjection));	
	Output.LightingFactor = dot(Normal, xLightDirection);
    Output.Normal = Normal;
	return Output;
}

PixelToFrame MovingObjectPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;	

	float4 color = PSIn.Color;

	//cell shading
	float intensity = dot(normalize(-xLightDirection), PSIn.Normal);

	if(intensity < 0)
        intensity = 0;

	if (intensity > 0.9)
        color = float4(1,1,1,1.0) * color;
    else if (intensity > 0.5)
        color = float4(0.9,0.9,0.9,1.0) * color;
	else if (intensity > 0.3)
        color = float4(0.8,0.8,0.8,1.0) * color;
    else 
        color = float4(0.7,0.7,0.7,1.0) * color;
	
	Output.Color = color;
		return Output;
}

VertexToPixel OutlineMovingObjectVS(float4 pos: POSITION0, float3 normal: NORMAL, float2 tex :TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);
	
	float4 mulNormal = normalize(mul(float4(normalize(normal),0),preWorldViewProjection));	
	float4 originalPosition = mul(pos, preWorldViewProjection); 

	Output.Position = originalPosition + (mul(0.1, mulNormal)); 

	return Output;    
}
 
PixelToFrame OutlineMovingObjectPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	Output.Color = float4(0,0,0,1);
    return Output;
}

technique MovingObjectShading
{
pass Pass0
	{
        CULLMODE = CW;
		VertexShader = compile vs_3_0 OutlineMovingObjectVS();
		PixelShader  = compile ps_3_0 OutlineMovingObjectPS();
	}

	pass Pass1
	{
        CULLMODE = CCW;
		VertexShader = compile vs_2_0 MovingObjectVS();
		PixelShader  = compile ps_2_0 MovingObjectPS();
	}
}


//---- Step7: Billboards	----------------------------------------------------------------------------------------------------------------------------------------------	Billboards

//unchanging constants
		

//changing constants
		float xBillboardWidth;
		float xBillboardHeight;
		float3 xAllowedRotDir;
		bool xToFlip;
		bool xToDraw;
		float4x4 xBillboardRotation;

		// 1 means we should only accept opaque pixels.
		// -1 means only accept transparent pixels.
		float xAlphaTestDirection = 1;
		float xAlphaTestThreshold = 0.7;

		bool xIsAnimated;
		float2 xFramePos;
		float2 xFrameDim;
		float3  xTranslation;

		Texture xBillboardTexture;
		sampler BillboardSampler = sampler_state { texture = <xBillboardTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};
		
			
VertexToPixel BillboardingVS(float4 pos: POSITION0, float3 texCoord: TEXCOORD)
{
	VertexToPixel Output = (VertexToPixel)0;
	
	float3 center = mul(pos + xTranslation, xWorldMatrix); 
	float3 eyeVector = center - xCameraPosition;
	float3 finalPosition = center;
	if(xToDraw)
	{
    float3 upVector = xAllowedRotDir;
    upVector = normalize(upVector);
    float3 sideVector = cross(eyeVector,upVector);
    sideVector = normalize(sideVector);

	float halfWidth = xBillboardWidth;
	float halfHeight = xBillboardHeight;
	sideVector *= halfWidth;
	upVector *= halfHeight;

    
	finalPosition -= upVector/2-0.2;
    finalPosition += (texCoord.x-0.5f)*sideVector;
    finalPosition += (1-texCoord.y)*upVector;
	}
    float4 finalPosition4 = float4(finalPosition, 1);

    float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	
    Output.Position = mul(finalPosition4, preViewProjection);

    Output.TextureCoords = texCoord;
	
	return Output;
}

PixelToFrame BillboardingPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;	
	float2 texCoord = PSIn.TextureCoords;

	if(xIsAnimated)
	{
		texCoord = float2(xFramePos.x + xFrameDim.x*texCoord.x, xFramePos.y + xFrameDim.y*texCoord.y);
	}

	float4 color;
		color = tex2D(BillboardSampler, texCoord);
	
	clip((color.a - xAlphaTestThreshold) * xAlphaTestDirection);
	Output.Color = color;
	return Output;
}

technique Billboarding
{
	pass Pass0
	{
	    CullMode = None;
        DestBlend         = One;       
		VertexShader = compile vs_3_0 BillboardingVS();
		PixelShader  = compile ps_3_0 BillboardingPS();
	}
}

//---- Step8: Checkpoint	----------------------------------------------------------------------------------------------------------------------------------------------	CheckPoint

			//unchanging constants
			float xCPSpriteSize;
			Texture xCPSpriteTexture;
			sampler CPSampler = sampler_state { texture = <xCPSpriteTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};
			

			float xCPState;

			//xCPState = 0 / reached
			//xCPState = 1 / next
			//xCPState = 2 / unchecked

			//changing constants
			float3 xCPSpritePosition;


VertexToPixel CheckpointVS(float4 pos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;
	pos = float4(xCPSpritePosition, pos.a);

	float3 center = mul(pos,xWorldMatrix);

	float3 eyeVector = center - xCameraPosition;

	float3 sideVector = cross(eyeVector, xCameraUp);
	sideVector = normalize(sideVector);

	float3 upVector = cross(sideVector, eyeVector);
	upVector = normalize(upVector);

	float3 finalPosition = center;
    finalPosition += (inTexCoord.x-0.5f)*sideVector*0.5f* xCPSpriteSize;
    finalPosition += (0.5f-inTexCoord.y)*upVector*0.5f* xCPSpriteSize;

    float4 finalPosition4 = float4(finalPosition, 1);

	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	
	Output.Position = mul(finalPosition4, preViewProjection);;
	

	//Output.Color = color;
	if(xCPState == 0)
		Output.Color = float4(1,1,1,1);

	if(xCPState == 1)
		Output.Color = float4(1,1,0,1);

	if(xCPState == 2)
		Output.Color = float4(1,0,0,1);
	
	Output.TextureCoords = inTexCoord;
	return Output;
}

PixelToFrame CheckpointPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;	

	float4 maskColor = tex2D(CPSampler, PSIn.TextureCoords); 
	Output.Color = float4(PSIn.Color.r*maskColor.a,PSIn.Color.g*maskColor.a,PSIn.Color.b*maskColor.a,maskColor.a); 

	return Output;
}

technique CheckpointShading
{
	pass Pass0
	{
	        AlphaBlendEnable  = true;            
        SrcBlend          = SrcAlpha; 
        DestBlend         = One;              
        ZWriteEnable      = false;

		VertexShader = compile vs_2_0 CheckpointVS();
		PixelShader  = compile ps_2_0 CheckpointPS();
	}
}


//---- Step9: MenuCanvas	----------------------------------------------------------------------------------------------------------------------------------------------	MenuCanvas

//unchanging constants
			Texture xCanvasTexture;
			sampler CanvasSampler = sampler_state { texture = <xCanvasTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};
			

//changing constants
			float3 xCanvasPosition;
			float xCanvasOpacity;
			bool xCanvasIsMask;
			float3 xCanvasColor;


VertexToPixel MenuCanvasVS(float4 pos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;
	
	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xInvertedViewMatrix, preViewProjection);

	pos.x += xCanvasPosition.x;
	pos.y += xCanvasPosition.y;
	pos.z += xCanvasPosition.z;

	Output.Position = mul(pos, preWorldViewProjection);	
    Output.TextureCoords = inTexCoord;
	if(xCanvasIsMask)
		Output.Color = float4(xCanvasColor.r,xCanvasColor.g,xCanvasColor.b,1); 
	return Output;
}

PixelToFrame MenuCanvasPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;	
	if(xCanvasIsMask)
	{
	float4 maskColor = tex2D(CanvasSampler, PSIn.TextureCoords); 
		Output.Color = float4(PSIn.Color.r*maskColor.a,PSIn.Color.g*maskColor.a,PSIn.Color.b*maskColor.a, maskColor.a); 
	}
	else
		Output.Color = tex2D(CanvasSampler, PSIn.TextureCoords);
	
	Output.Color *=  xCanvasOpacity;
	
	return Output;
}

technique MenuCanvasShading
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 MenuCanvasVS();
		PixelShader  = compile ps_2_0 MenuCanvasPS();
	}
}

//---- Step10: Time	----------------------------------------------------------------------------------------------------------------------------------------------	Time

//unchanging constants
			Texture xZero;
			Texture xOne;
			Texture xTwo;
			Texture xThree;
			Texture xFour;
			Texture xFive;;
			Texture xSix;
			Texture xSeven;
			Texture xEight;
			Texture xNine;

			sampler SamplerFor0 = sampler_state { texture = <xZero> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = wrap; AddressV = wrap;};
			sampler SamplerFor1 = sampler_state { texture = <xOne> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = wrap; AddressV = wrap;};
			sampler SamplerFor2 = sampler_state { texture = <xTwo> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = wrap; AddressV = wrap;};
			sampler SamplerFor3 = sampler_state { texture = <xThree> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = wrap; AddressV = wrap;};
			sampler SamplerFor4 = sampler_state { texture = <xFour> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = wrap; AddressV = wrap;};
			sampler SamplerFor5 = sampler_state { texture = <xFive> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = wrap; AddressV = wrap;};
			sampler SamplerFor6 = sampler_state { texture = <xSix> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = wrap; AddressV = wrap;};
			sampler SamplerFor7 = sampler_state { texture = <xSeven> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = wrap; AddressV = wrap;};
			sampler SamplerFor8 = sampler_state { texture = <xEight> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = wrap; AddressV = wrap;};
			sampler SamplerFor9 = sampler_state { texture = <xNine> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = wrap; AddressV = wrap;};

//changing constants
			float3 xTimePosition;
			float xNumber;
			bool xShowNumbers;
			float xTimePlate;
			bool xInWorldSpace;


VertexToPixel TimeVS(float4 pos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;
	float3 center;
	float4x4 preViewProjection;

	if(xInWorldSpace)
	{
	center = mul(xTimePosition,xWorldMatrix);

	float3 eyeVector = center - xCameraPosition;

	float3 sideVector = cross(eyeVector, xCameraUp);
	sideVector = normalize(sideVector);

	float3 upVector = cross(sideVector, eyeVector);
	upVector = normalize(upVector);

	float3 finalPosition = center;
    finalPosition += sideVector* pos.x;
    finalPosition += upVector* pos.y;

    float4 finalPosition4 = float4(finalPosition, 1);

	preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	
	Output.Position = mul(finalPosition4, preViewProjection);

	//Output.Color = color;
	if(xCPState == 0)
		Output.Color = float4(1,1,1,1);

	if(xCPState == 1)
		Output.Color = float4(1,1,0,1);

	if(xCPState == 2)
		Output.Color = float4(1,0,0,1);

	}
	else
	{

	preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xInvertedViewMatrix, preViewProjection);
	pos.x += xTimePosition.x;
	pos.y += xTimePosition.y;
	pos.z += xTimePosition.z;
	Output.Position = mul(pos, preWorldViewProjection);	
    Output.TextureCoords = inTexCoord;
	}
	Output.Color = float4(1, 1*xTimePlate/5 ,0,1);
	return Output;
}

PixelToFrame TimePS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;	
	PSIn.TextureCoords.y = 1-PSIn.TextureCoords.y;
	float4 TextureColor;
	if(xNumber == 0)
	TextureColor = tex2D(SamplerFor0, PSIn.TextureCoords);
	if(xNumber == 1)
	TextureColor = tex2D(SamplerFor1, PSIn.TextureCoords);
	if(xNumber == 2)
	TextureColor = tex2D(SamplerFor2, PSIn.TextureCoords);
	if(xNumber == 3)
	TextureColor = tex2D(SamplerFor3, PSIn.TextureCoords);
	if(xNumber == 4)
	TextureColor = tex2D(SamplerFor4, PSIn.TextureCoords);
	if(xNumber == 5)
	TextureColor = tex2D(SamplerFor5, PSIn.TextureCoords);
	if(xNumber == 6)
	TextureColor = tex2D(SamplerFor6, PSIn.TextureCoords);
	if(xNumber == 7)
	TextureColor = tex2D(SamplerFor7, PSIn.TextureCoords);
	if(xNumber == 8)
	TextureColor = tex2D(SamplerFor8, PSIn.TextureCoords);
	if(xNumber == 9)
	TextureColor = tex2D(SamplerFor9, PSIn.TextureCoords);

	Output.Color =  TextureColor;
	
	return Output;
}

technique TimeShading
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 TimeVS();
		PixelShader  = compile ps_2_0 TimePS();
	}
}

//---- Extended: ColoredModel	----------------------------------------------------------------------------------------------------------------------------------------------	ColoredModel

			//unchanging constants

			//changing constants


VertexToPixel ColoredModelVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);
    float4 newColor = inColor;
	//if(inPos.x>2) inPos.x = 0;
	///if(inPos.z>2) inPos.z = 0;
	//inPos.y = 0;
	Output.Position = mul(inPos, preWorldViewProjection);
	
	Output.Color = newColor;
	float3 Normal = normalize(mul(normalize(inNormal), xWorldMatrix));	
	Output.LightingFactor = 1;
	Output.LightingFactor = dot(Normal, -xLightDirection);
	Output.LightingFactor2 = dot(Normal, xLightDirection);
    
	return Output;    
}

PixelToFrame ColoredModelPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	Output.Color = PSIn.Color;
	Output.Color.rgb *= saturate(PSIn.LightingFactor) + saturate(PSIn.LightingFactor2) + xAmbient;
	return Output;
}

technique ColoredModelShading
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 ColoredModelVS();
		PixelShader  = compile ps_2_0 ColoredModelPS();
	}
}



//------- Technique: Pretransformed --------

VertexToPixel PretransformedVS( float4 inPos : POSITION, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	
	Output.Position = inPos;
	Output.Color = inColor;
    
	return Output;    
}

PixelToFrame PretransformedPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = PSIn.Color;

	return Output;
}

technique Pretransformed
{
	pass Pass0
	{   
		VertexShader = compile vs_2_0 PretransformedVS();
		PixelShader  = compile ps_2_0 PretransformedPS();
	}
}