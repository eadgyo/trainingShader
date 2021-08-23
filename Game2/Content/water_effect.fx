uniform extern float4x4 View;
uniform extern float4x4 Projection;
uniform extern float4x4 World;
uniform extern float3 gCameraPosition;
uniform extern float4x4 gReflectionView;
uniform extern Texture2D gReflectionMap;
uniform extern float3 gBaseColor;
uniform extern float gBaseColorAmount;

uniform extern float gViewportWidth;
uniform extern float gViewportHeight;

static float WaveLength = 0.1f;
static float WaveHeight = 0.1;
uniform extern float gTime;
static float WaveSpeed = 0.2f;
uniform extern Texture2D gWaterNormalTexture;
uniform extern float3 gSunDirection;

sampler2D waterNormalSampler = sampler_state
{
	texture = <gWaterNormalTexture>;
	AddressU = mirror;
	AddressV = mirror;
	MinFilter = Point;
	MagFilter = Point;
};

sampler2D reflectionSampler = sampler_state
{
	texture = <gReflectionMap>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	AddressU = mirror;
	AddressV = mirror;
};


struct OutputVS
{
	float4 posH : POSITION0;
	float4 posR : TEXCOORD0;
	float2 tex0 : TEXCOORD1;
	float2 normalMapTex : TEXCOORD2;
	float4 worldPos : TEXCOORD3;
};

float2 postProjToScreen(float4 position)
{
	float2 screenPos = position.xy / position.w;
	return 0.5f * (float2(screenPos.x, -screenPos.y) + 1);
}

float2 halfPixel()
{
	return 0.5f / float2(gViewportWidth, gViewportHeight);
}

OutputVS TransformVS(float3 posL : POSITION0, float2 tex : TEXCOORD0)
{
	// Zero out our output
	OutputVS outVS = (OutputVS)0;

	float4x4 wvp = mul(World, mul(View, Projection));
	outVS.posH = mul(float4(posL, 1.0f), wvp);

	// Transform to reflection space
	float4x4 rWVP = mul(World, mul(gReflectionView, Projection));
	outVS.posR = mul(float4(posL, 1.0f), rWVP);
	outVS.tex0 = tex;

	outVS.normalMapTex = tex / WaveLength;
	outVS.normalMapTex.y -= gTime * WaveSpeed;
	outVS.worldPos = mul(float4(posL, 1.0f), World);
	return outVS;
}

float4 TransformPS(float4 posR : TEXCOORD0, float2 tex0 : TEXCOORD1, float2 normalMapTex : TEXCOORD2, float4 worldPos : TEXCOORD3) : COLOR
{
	// Normal offset
	float2 reflectionUV = postProjToScreen(posR) + halfPixel();
	float4 normal = tex2D(waterNormalSampler, normalMapTex) * 2 - 1;
	float2 UVOffset = WaveHeight * normal.rg;

	// Tex UV
	float2 ProjectedTexCoords;
	ProjectedTexCoords.x = posR.x / posR.w / 2.0f + 0.5f;
	ProjectedTexCoords.y = -posR.y / posR.w / 2.0f + 0.5f;
	float4 reflection = tex2D(reflectionSampler, ProjectedTexCoords + UVOffset);

	// Specular effect
	float3 sunDirection = normalize(gSunDirection);
	float3 viewDirection = normalize(gCameraPosition - worldPos);
	float3 reflectionVector = -reflect(sunDirection, normal.rgb);
	float specular = dot(normalize(reflectionVector), viewDirection);
	specular = pow(specular, 256);

	return float4(lerp(reflection.rgb, gBaseColor, gBaseColorAmount) + specular, 1);
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_2_0 TransformVS();
		pixelShader = compile ps_3_0 TransformPS();
	}
};