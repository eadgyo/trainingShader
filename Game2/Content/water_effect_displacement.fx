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
static float DisWaveLength = 0.1f;
static float WaveHeight = 0.2;
uniform extern float gTime;
static float WaveSpeed = 0.2f;
static float DisWaveSpeed = 2.0f;
uniform extern Texture2D gWaterNormalTexture;
uniform extern float3 gSunDirection;
static float yDisplacement = -150.0f;


uniform extern Texture2D gDisTex0;
uniform extern Texture2D gDisTex1;
uniform extern float DMAP_SIZE;
uniform extern float2 gScaleHeights;
uniform extern float gCellSize;

uniform extern bool gUseSpecular;

sampler2D samplerDisTex0 = sampler_state
{
	texture = <gDisTex0>;
	AddressU = mirror;
	AddressV = mirror;
	MinFilter = Point;
	MagFilter = Point;
};

sampler2D samplerDisTex1 = sampler_state
{
	texture = <gDisTex1>;
	AddressU = mirror;
	AddressV = mirror;
	MinFilter = Point;
	MagFilter = Point;
};

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
	float3 worldPos : TEXCOORD3;
	float3 toEyeT : TEXCOORD4;
	float3 sunDirectionT : TEXCOORD5;
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

float DoDispMapping(float2 texC0, float2 texC1)
{
	// Transform to texel space
	float2 texelPos = DMAP_SIZE * texC0;

	// Determine the lerp amounts
	float2 lerps = frac(texelPos);

	float dmap_dx = 1.0f / DMAP_SIZE;

	// Filter displacement
	float dmap[4];
	dmap[0] = tex2Dlod(samplerDisTex0, float4(texC0, 0.0f, 0.0f)).r;
	dmap[1] = tex2Dlod(samplerDisTex0, float4(texC0, 0.0f, 0.0f) + float4(dmap_dx, 0.0f, 0.0f, 0.0f)).r;
	dmap[2] = tex2Dlod(samplerDisTex0, float4(texC0, 0.0f, 0.0f) + float4(0.0f, dmap_dx, 0.0f, 0.0f)).r;
	dmap[3] = tex2Dlod(samplerDisTex0, float4(texC0, 0.0f, 0.0f) + float4(dmap_dx, dmap_dx, 0.0f, 0.0f)).r;

	float lerp0 = lerp(dmap[0], dmap[1], lerps.x);
	float lerp1 = lerp(dmap[2], dmap[3], lerps.x);

	float h0 = lerp(lerp0, lerp1, lerps.y);

	// Second displacement
	texelPos = DMAP_SIZE * texC1;

	// Determine the lerp amounts
	lerps = frac(texelPos);

	dmap[0] = tex2Dlod(samplerDisTex1, float4(texC1, 0.0f, 0.0f)).r;
	dmap[1] = tex2Dlod(samplerDisTex1, float4(texC1, 0.0f, 0.0f) + float4(dmap_dx, 0.0f, 0.0f, 0.0f)).r;
	dmap[2] = tex2Dlod(samplerDisTex1, float4(texC1, 0.0f, 0.0f) + float4(0.0f, dmap_dx, 0.0f, 0.0f)).r;
	dmap[3] = tex2Dlod(samplerDisTex1, float4(texC1, 0.0f, 0.0f) + float4(dmap_dx, dmap_dx, 0.0f, 0.0f)).r;

	lerp0 = lerp(dmap[0], dmap[1], lerps.x);
	lerp1 = lerp(dmap[2], dmap[3], lerps.x);

	float h1 = lerp(lerp0, lerp1, lerps.y);

	return gScaleHeights.x * h0 + gScaleHeights.y * h1;
}


OutputVS TransformVS(float3 posL : POSITION0, float2 tex : TEXCOORD0)
{
	// Zero out our output
	OutputVS outVS = (OutputVS)0;

	float2 tex01 = tex / DisWaveLength;
	tex01.y -= gTime * DisWaveSpeed;

	posL.y = posL.y + DoDispMapping(tex01, tex01) + yDisplacement;

	// Estimate TBN-basis using finite differencing in local sapce
	float dmap_dx = 1.0f / DMAP_SIZE;
	float r = DoDispMapping(tex01 + float2(dmap_dx, 0.0f), tex01 + float2(dmap_dx, 0));
	float b = DoDispMapping(tex01 + float2(dmap_dx, 0.0f), tex01 + float2(dmap_dx, 0));
	
	float3x3 TBN;
	TBN[0] = normalize(
		float3(1.0f, (r - posL.y) / gCellSize, 0.0f)
	);
	TBN[1] = normalize(
		float3(0.0f, (b - posL.y) / gCellSize, -1.0f)
	);
	TBN[2] = normalize(cross(TBN[0], TBN[1]));

	float3x3 toTangentSpace = transpose(TBN);

	// gCameraPosition is already in local space
	// float3 eyePosL = mul(float4(gEyePosW,1.0f), gWorldInv)).xyz
	float3 toEyeL = gCameraPosition;
	outVS.toEyeT = mul(toEyeL, toTangentSpace);
	// Same with light direction with tangent space
	outVS.sunDirectionT = mul(gSunDirection, toTangentSpace);

	float4x4 wvp = mul(World, mul(View, Projection));
	outVS.posH = mul(float4(posL, 1.0f), wvp);

	// Transform to reflection space
	float4x4 rWVP = mul(World, mul(gReflectionView, Projection));
	outVS.posR = mul(float4(posL, 1.0f), rWVP);
	outVS.tex0 = tex;

	outVS.normalMapTex = tex / WaveLength;
	outVS.normalMapTex.y -= gTime * WaveSpeed;
	outVS.worldPos = mul(float4(posL, 1.0f), TBN).xyz;

	return outVS;
}


float4 TransformPS(OutputVS input) : COLOR
{
	// Normal offset
	float2 reflectionUV = postProjToScreen(input.posR) + halfPixel();
	float4 normal = tex2D(waterNormalSampler, input.normalMapTex) * 2 - 1;
	float2 UVOffset = WaveHeight * normal.rg;

	// Tex UV
	float2 ProjectedTexCoords;
	ProjectedTexCoords.x = input.posR.x / input.posR.w / 2.0f + 0.5f;
	ProjectedTexCoords.y = -input.posR.y / input.posR.w / 2.0f + 0.5f;
	float4 reflection = tex2D(reflectionSampler, ProjectedTexCoords + UVOffset);

	// Specular effect
	float3 sunDirection = normalize(input.sunDirectionT);
	float3 viewDirection = normalize(input.toEyeT - input.worldPos);
	float3 reflectionVector = -reflect(sunDirection, normal.rgb);
	float specular1 = dot(normalize(reflectionVector), viewDirection);
	float specular = 0.0f;
	if (specular1 > 0.0f && gUseSpecular)
	{
		specular = pow(specular1, 128) * 2;
	}

	return float4(lerp(reflection.rgb, gBaseColor, gBaseColorAmount) + specular, 1);
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_4_0 TransformVS();
		pixelShader = compile ps_4_0 TransformPS();
	}
};