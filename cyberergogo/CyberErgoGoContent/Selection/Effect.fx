struct VertexToPixel
{
    float4 Position   	: POSITION;    
    float4 Color		: COLOR0;
	float LightingFactor: TEXCOORD0;
    float2 TextureCoords: TEXCOORD1;
	float LightingFactor2: TEXCOORD2;
	float4 LevelWeight: TEXCOORD3;
	float StreetWeight: TEXCOORD4;
	float3 Normal:TEXCOORD5;
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
float4x4 xInvertedViewMatrix;
float3 xCameraLookAt;
float3 xCameraPosition;

float2 xPositionAsTextureCoord;

//------- Technique: TerrainShading ----------------------------------------------------------------------------------------------------------------------- TerrainShading

Texture xTerrainPreviewTextureOld;
float xTerrainPreviewCTHRateOld;
Texture xTerrainPreviewTextureCurrent;
float xTerrainPreviewCTHRateCurrent;

Texture xLevel0Texture;
Texture xLevel1Texture;
Texture xLevel2Texture;
Texture xLevel3Texture;
Texture xStreetTexture;
float xLevel0to1;
float xLevel1to2;
float xLevel2to3;

float xPreviewWidth;
float xPreviewHeight;
float xMorphingFactor;
float xColorToHeightRate;

sampler TerrainSamplerOld = sampler_state{
			texture = <xTerrainPreviewTextureOld>;
			MipFilter = Point;
			MinFilter = Point;
			MagFilter = Point;
			AddressU  = Wrap;
			AddressV  = Wrap;
		};

sampler TerrainSamplerCurrent = sampler_state{
			texture = <xTerrainPreviewTextureCurrent>;
			MipFilter = Point;
			MinFilter = Point;
			MagFilter = Point;
			AddressU  = Wrap;
			AddressV  = Wrap;
		};

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

float4 GetMorphedHeight(float texX, float texY)
{
	float4 heightCurrent = tex2Dlod(TerrainSamplerCurrent,float4(texX,texY,0,0))/5;
	float4 heightOld = tex2Dlod(TerrainSamplerOld,float4(texX,texY,0,0))/5;
	float4 result = ((xMorphingFactor*heightCurrent) + (1-xMorphingFactor)*heightOld);
	return result;
}

float4 GetMorphedHeightWithoutHRate(float texX, float texY)
{
	float4 heightCurrent = tex2Dlod(TerrainSamplerCurrent,float4(texX,texY,0,0));
	float4 heightOld = tex2Dlod(TerrainSamplerOld,float4(texX,texY,0,0));
	float4 result = ((xMorphingFactor*heightCurrent) + (1-xMorphingFactor)*heightOld);
	result.b = max(heightCurrent.g, heightOld.g);
	if(heightCurrent.g == 1)
	{
		result.b = 1;
		result.g = 1 * xMorphingFactor;
	}
	if(heightOld.g == 1)
	{
		result.b = 1;
		result.g = 1 * (1-xMorphingFactor);
	}
	return result;
}

float4 GetMorphedColor(float texX, float texY)
{
	float4 heightCurrent = tex2D(TerrainSamplerCurrent,float2(texX,texY));
	float4 heightOld = tex2D(TerrainSamplerOld,float2(texX,texY));

	return ((xMorphingFactor*heightCurrent) + (1-xMorphingFactor)*heightOld);
}

float3 CalculatedNormal(float4 inPos, float texX, float texY){
			//Nachbarpunkte zusammenstellen
			//Kanten berechnen
			//Flächennormalen berechnen
			//Mittelwert bilden
	
			//  pattern:

			//  ^
			//  | Z / TexY

			//	n1 - n2       
			//	| \  |  \
			//	n3 - p  - n4
			//	  \  | \  | 
			//	    n5 - n6     -> X / TexX

			float xNextXZ = 1;

			float deltaXZ = xNextXZ;

			float nextTexX = 1/(xPreviewWidth);
			float nextTexY = 1/(xPreviewHeight);

			float texYnone = texY;
			float texYplus = texYnone+nextTexY;
			float texYminus = texYnone-nextTexY;

			//point
			float3 p = inPos;

			//calculate neighbors
			float3 n1 = float3(0,0,0);
			float3 n2 = float3(0,0,0);
			float3 n3 = float3(0,0,0);
			float3 n4 = float3(0,0,0);
			float3 n5 = float3(0,0,0);
			float3 n6 = float3(0,0,0);
			

			//if (texYplus <= 1 && (texX-nextTexX)>=0)
			n1 = float3(inPos.x-deltaXZ, GetMorphedHeight(texX-nextTexX,texYplus).x*51, inPos.z+deltaXZ);
			
			//if (texYplus <= 1)
			n2 = float3(inPos.x, GetMorphedHeight(texX,texYplus).x*51, inPos.z+deltaXZ);
			
			//if ((texX-nextTexX) >= 0)
			n3 = float3(inPos.x-deltaXZ, GetMorphedHeight(texX-nextTexX,texYnone).x*51, inPos.z);
			
			//if ((texX+nextTexX) <= 1)
			n4 = float3(inPos.x+deltaXZ, GetMorphedHeight(texX+nextTexX,texYnone).x*51, inPos.z);

			//if (texYminus >= 0)
			n5 = float3(inPos.x, GetMorphedHeight(texX,texYminus).x*51, inPos.z-deltaXZ);
			
			//if (texYminus >= 0 && (texX+nextTexX) <= 1)
			n6 = float3(inPos.x+deltaXZ, GetMorphedHeight(texX+nextTexX,texYminus).x*51, inPos.z-deltaXZ);
	
			//calculate face normals
	
			float3 nl1 = float3(0,0,0);
			float3 nl2 = float3(0,0,0);
			float3 nl3 = float3(0,0,0);
			float3 nl4 = float3(0,0,0);
			float3 nl5 = float3(0,0,0);
			float3 nl6 = float3(0,0,0);

			//  ^
			//  | Z / TexY

			//	n1 - n2       
			//	| \  |  \
			//	n3 - p  - n4
			//	  \  | \  | 
			//	    n5 - n6     -> X / TexX

			//if((texX-nextTexX) >= 0 && texYplus <= 1)
				nl1 = cross((n1-p),(n2-p));
			
			//if((texX+nextTexX) <= 1 && texYplus <= 1)
				nl2 = cross((n2-p),(n4-p));

			//if((texX+nextTexX) <= 1 && texYminus >= 0)
				nl3 = cross((n4-p),(n6-p));

			//if((texX+nextTexX) <= 1 && texYminus >= 0)
				nl4 = cross((n6-p),(n5-p));
			
			//if((texX-nextTexX) >= 0 && texYminus >= 0)
				nl5 = cross((n5-p),(n3-p));

			//if((texX-nextTexX) >= 0 && texYplus <= 1)
				nl6 = cross((n3-p),(n1-p));

			return normalize(nl1+nl2+nl3+nl4+nl5+nl6);
		}



VertexToPixel TerrainVS(float4 inPos : POSITION, float2 inTexCoords: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);
    
	float ShowStreetOn = 0.8;
	float HideStreetTo = 1;
	float4 h_color = GetMorphedHeight(inTexCoords.x,inTexCoords.y);
	inPos.y = h_color.r*255;
	float3 inNormal = CalculatedNormal(inPos,inTexCoords.x, inTexCoords.y);
	//inNormal = float3(0,1,0);

	Output.Position = mul(inPos, preWorldViewProjection);

	if(h_color.g/1 == 1 && h_color.b==0)
		Output.Color = float4(0.5,0.5,0,1);
	else
	{
	if(xMorphingFactor < ShowStreetOn)
		Output.Color = float4(h_color.r/1,h_color.r/1,h_color.r/1,1);
		else
		{
		if(xMorphingFactor >= ShowStreetOn)
		{
				float4 morphingValue = (xMorphingFactor - ShowStreetOn)/(1-ShowStreetOn);
				float4 morphedColor = float4(h_color.r/1,h_color.g/1,h_color.b/1,1)*(morphingValue) + float4(h_color.r/1,h_color.r/1,h_color.r/1,1)*(1-morphingValue);
			    Output.Color = morphedColor;
			}
			//else
			//{
			//	float4 morphingValue = xMorphingFactor/HideStreetTo;
			//	float4 morphedColor = float4(h_color.r/1,h_color.g/1,h_color.b/1,1)*(1-morphingValue) + float4(h_color.r/1,h_color.r/1,h_color.r/1,1)*(morphingValue);
			//	Output.Color = morphedColor;
			//}
		}
	}
	
	float4 Normal = normalize(mul(float4(normalize(inNormal),0),xWorldMatrix));	

	Output.Normal = float3(Normal.x,Normal.y,Normal.z);

	Output.TextureCoords = inTexCoords;
	return Output;    
}

PixelToFrame TerrainPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		

	float4 level0 = tex2D(Level0Sampler, PSIn.TextureCoords*2);
	float4 level1 = tex2D(Level1Sampler, PSIn.TextureCoords*2);
	float4 level2 = tex2D(Level2Sampler, PSIn.TextureCoords*2);
	float4 level3 = tex2D(Level3Sampler, PSIn.TextureCoords*2);
	float4 street = tex2D(StreetSampler, PSIn.TextureCoords*50);

	float4 LevelWeight;

	float4 terrainColor = GetMorphedColor(PSIn.TextureCoords.x,PSIn.TextureCoords.y);
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


VertexToPixel OutlineTerrainVS(float4 inPos : POSITION, float2 inTexCoords: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);
    
	float ShowStreetOn = 0.8;
	float HideStreetTo = 1;
	float4 h_color = GetMorphedHeight(inTexCoords.x,inTexCoords.y);
	inPos.y = h_color.r*255;
	
	float3 inNormal = CalculatedNormal(inPos,inTexCoords.x, inTexCoords.y);
	
	float4 normal = normalize(mul(float4(normalize(inNormal),0),preWorldViewProjection));	
	float4 originalPosition = mul(inPos, preWorldViewProjection); 

	Output.Position = originalPosition + (mul(0.1, normal)); 

	return Output;    
}
 
PixelToFrame OutlineTerrainPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	Output.Color = float4(0,0,0,1);
    return Output;
}

technique TerrainShading
{
	pass Pass0
	{
		ZENABLE = TRUE;
        ZWRITEENABLE = TRUE;
        CULLMODE = CW;
		VertexShader = compile vs_3_0 OutlineTerrainVS();
		PixelShader  = compile ps_3_0 OutlineTerrainPS();
	}
	
	pass Pass1
	{
		ZENABLE = TRUE;
         ZWRITEENABLE = TRUE;
         CULLMODE = CCW;
		VertexShader = compile vs_3_0 TerrainVS();
		PixelShader  = compile ps_3_0 TerrainPS();
	}
}

//---- Step3: TracPreview	----------------------------------------------------------------------------------------------------------------------------------------------	TracPreview

			//unchanging constants
			float4 xTracPreviewColor;

			//changing constants

float4 GetCurrentHeight(float texX, float texY)
{
	float4 heightCurrent = tex2Dlod(TerrainSamplerCurrent,float4(texX,texY,0,0))/xTerrainPreviewCTHRateCurrent;
	float4 result = xMorphingFactor*heightCurrent;
	return result;
}


VertexToPixel TracPreviewVS(float4 pos : POSITION, float3 normal: NORMAL, float2 inTexCoords: TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;

	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);
	
	 Output.Color = xTracPreviewColor;

	float4 terrainValue = GetMorphedHeight(xPositionAsTextureCoord.x,xPositionAsTextureCoord.y);
	float newY = terrainValue.r;

	bool contour = false;
	if(inTexCoords.x > 0.05 && inTexCoords.x < 0.95 && inTexCoords.y > 0.05 && inTexCoords.y < 0.95)
	{
		contour = true;
	}

	if(inTexCoords.x > 0.1 && inTexCoords.x < 0.9 && inTexCoords.y > 0.1 && inTexCoords.y < 0.9)
	{
		float2 tex = inTexCoords-((40*inTexCoords) % 1)/40;
		//float2 tex = inTexCoords;
    
		float4 h_color = GetMorphedHeightWithoutHRate(tex.x,tex.y);


		if(h_color.b == 1)
		{
			pos += float4(normal*h_color.g*0.1, 0);
			Output.Color = float4(0,0.4*(h_color.r),0,1)*(1-h_color.g) + float4(0.5,0.5,0.5,1) * (h_color.g);
		}
		else
		Output.Color = float4(0,0.4,0,1) * h_color.g;
		Output.Color += 0.3;
		contour = false;
	}
	if(contour)
	Output.Color = float4(0,0,0,1);


	Output.Position = mul(pos, preWorldViewProjection);
	Output.Position.y +=  newY*255;
	//Output.Color = color;

	//if(inTexCoords.x == 1 && inTexCoords.y == 1)
	//	color = float4(1, 0, 0, 1);
	
	//if(inTexCoords.x == 0 && inTexCoords.y == 0)
	//	color = float4(0, 1, 0, 1);

	float3 Normal = normalize(mul(normalize(normal), xWorldMatrix));	
	Output.Normal = Normal;

	Output.TextureCoords = inTexCoords;
    
	return Output;
}

PixelToFrame TracPreviewPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;	

	float intensity = dot(normalize(-float3(xLightDirection.x,0.5,xLightDirection.z)), PSIn.Normal);
	float4 color = PSIn.Color;
	//float4 streetColor = tex2Dlod(TerrainSamplerCurrent,float4(PSIn.TextureCoords.x,PSIn.TextureCoords.y,0,0));

	//if(streetColor.g == 1)
	//	color = streetColor;

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

	
	//Output.Color.rgb *= saturate(PSIn.LightingFactor) + saturate(PSIn.LightingFactor2) + xAmbient;

	return Output;
}

VertexToPixel OutlineTracPreviewVS(float4 pos : POSITION, float3 normal: NORMAL, float4 color: COLOR, float2 inTexCoords: TEXCOORD0)
{
	VertexToPixel Output = (VertexToPixel)0;
	float3 colorNormal = float3(color.r,color.g,color.b) - 0.5;
	float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
	float4x4 preWorldViewProjection = mul (xWorldMatrix, preViewProjection);

	if(inTexCoords.x > 0.1 && inTexCoords.x < 0.9 && inTexCoords.y > 0.1 && inTexCoords.y < 0.9)
	{
	float2 tex = inTexCoords;
    
	float4 h_color = GetMorphedHeightWithoutHRate(tex.x,tex.y);
	if(h_color.b == 1)
		pos += float4(normal*h_color.g*0.2, 0);
	
	}

    float4 originalPosition = mul(pos, preWorldViewProjection); 
	Output.Position = originalPosition + (mul(0.05, float4(normalize(colorNormal),0))); 

	float4 terrainValue = GetMorphedHeight(xPositionAsTextureCoord.x,xPositionAsTextureCoord.y);
	float newY = terrainValue.r; 
	Output.Position.y +=  newY*255;
	
	float4 newNormal = normalize(mul(float4(normalize(normal),0),xWorldMatrix));	
	return Output;
}

PixelToFrame OutlineTracPreviewPS(VertexToPixel PSIn) : COLOR0
{
	PixelToFrame Output = (PixelToFrame)0;		
	Output.Color = float4(0,0,0,1);
    return Output;
}

technique TracPreviewShading
{
	pass Pass0
	{
		ZENABLE = TRUE;
        ZWRITEENABLE = TRUE;
        CULLMODE = CW;
		VertexShader = compile vs_3_0 OutlineTracPreviewVS();
		PixelShader  = compile ps_3_0 OutlineTracPreviewPS();
	}
	
	pass Pass1
	{
		ZENABLE = TRUE;
         ZWRITEENABLE = TRUE;
         CULLMODE = CCW;
		VertexShader = compile vs_3_0 TracPreviewVS();
		PixelShader  = compile ps_3_0 TracPreviewPS();
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
	Output.Color = cloudColor;
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
	float fogDis = 0.2;
	float2 middle = float2(0.5,0.5);
	float dis = length(middle-PSIn.TextureCoords);
	if(dis<fogDis) 
		dis = 0;
	else
		dis = (dis-fogDis)/0.3;
	if(dis > 1) dis = 1;
	Output.Color =  GetSkyMirage(PSIn.TextureCoords, PSIn.Color)*(1-dis) + dis*float4(0.77,0.956,0.941,1);
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
        color = float4(0.7,0.7,0.7,1.0) * color;
	else if (intensity > 0.3)
        color = float4(0.4,0.4,0.4,1.0) * color;
    else 
        color = float4(0.2,0.2,0.2,1.0) * color;
	
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
		float xGrowingFactor;
		bool xToFlip;

		// 1 means we should only accept opaque pixels.
		// -1 means only accept transparent pixels.
		float xAlphaTestDirection = 1;
		float xAlphaTestThreshold = 0.7;

		bool xSetOnTerrain;
		
		//for animated billboards
		bool xIsAnimated;
		float2 xFramePos;
		float2 xFrameDim;

		float3 xTranslation;

		Texture xBillboardTexture;
		sampler BillboardSampler = sampler_state { texture = <xBillboardTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};
		
			
VertexToPixel BillboardingVS(float4 pos: POSITION0, float3 texCoord: TEXCOORD)
{
	VertexToPixel Output = (VertexToPixel)0;
	bool doBillboarding = true;

	if(xSetOnTerrain)
	{
			float4 terrainColor = GetMorphedHeightWithoutHRate(xPositionAsTextureCoord.x,xPositionAsTextureCoord.y);
			if(terrainColor.b != 1 && terrainColor.r > xLevel0to1-0.1 && terrainColor.r < xLevel1to2)
			{
			float4 terrainValue = GetMorphedHeight(xPositionAsTextureCoord.x,xPositionAsTextureCoord.y);
			float newY = terrainValue.r;
			pos.y = newY*255 + xBillboardHeight/2;
			}
			else
			doBillboarding = false;

	}

	float3 center = mul(pos + xTranslation, xWorldMatrix); 
	float3 eyeVector = center - xCameraPosition;

	if(doBillboarding)
	{
    float3 upVector = xAllowedRotDir;
    upVector = normalize(upVector);
    float3 sideVector = cross(eyeVector,upVector);
    sideVector = normalize(sideVector);

	float halfWidth = xBillboardWidth;
	float halfHeight = xBillboardHeight;
	sideVector *= halfWidth;
	upVector *= halfHeight;

    float3 finalPosition = center;
	finalPosition -= upVector/2-0.2;
    finalPosition += (texCoord.x-0.5f)*sideVector;
    finalPosition += (1-texCoord.y)*upVector*xGrowingFactor;

    float4 finalPosition4 = float4(finalPosition, 1);

    float4x4 preViewProjection = mul (xViewMatrix, xProjectionMatrix);
    Output.Position = mul(finalPosition4, preViewProjection);
	}

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

//---- Step8: MenuCanvas	----------------------------------------------------------------------------------------------------------------------------------------------	MenuCanvas

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
    Output.TextureCoords = float2(inTexCoord.x,1-inTexCoord.y);
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