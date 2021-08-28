uniform extern float4x4 gViewProj;
uniform extern float3 gPosCamera;
uniform extern float3 gDirToSunW;
uniform extern texture2D gTex0;
uniform extern float gTexScale;
uniform extern Texture2D gNoise;
uniform extern float gNoiseScale;
uniform extern float gNoiseDistance;
extern int gUseNoise = 0;

uniform extern texture gShadowMap;
uniform extern float4x4 gShadowView;
uniform extern float4x4 gShadowProjection;
uniform extern float gShadowFarPlane;
static float ShadowMult = 0.3f;
static float ShadowBias = 0.000005f;
uniform extern float SMAP_SIZE;

static float3 gFogColor = { 0.5f, 0.5f, 0.5f };
static float gFogStart = 1.0f;
static float gFogRange = 5000.0f;
uniform extern bool gFogEnabled;

uniform extern bool gClipPlaneEnabled = true;
uniform extern float4 gClipPlane;
uniform extern bool gUseShadowLerp;


sampler shadowMapSampler = sampler_state
{
	Texture = <gShadowMap>;
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = POINT;
	AddressU = CLAMP;
	AddressV = CLAMP;
};


sampler gTex0Sampler = sampler_state 
{
	Texture = <gTex0>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

sampler gNoiseSampler = sampler_state
{
	Texture = <gNoise>;
	AddressU = Wrap;
	AddressV = Wrap;
	MinFilter = Linear;
	MagFilter = Linear;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float Depth : TEXCOORD2;
	float4 posW : TEXCOORD3;
	float4 shadowScreenPosition : TEXCOORD4;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = mul(input.Position, gViewProj);
	output.Normal = input.Normal;
	output.UV = input.UV;
	float3 vec = gPosCamera - input.Position;
	output.Depth = sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
	output.posW = input.Position;
	// posW = mul(input.Position, World) World == Identity
	float4x4 ViewProjShadow = mul(gShadowView, gShadowProjection);
	output.shadowScreenPosition = mul(input.Position, ViewProjShadow);
	return output;
}

float samplerShadowMap(float2 UV)
{
	if (UV.x < 0 || UV.x > 1 || UV.y < 0 || UV.y > 1)
		return 0;

	return tex2D(shadowMapSampler, UV);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	if (gClipPlaneEnabled)
		clip(dot(input.posW, gClipPlane));

	float4 projTex = input.shadowScreenPosition;

	projTex.xy /= projTex.w;
	// Convert from [-1, 1] to [0, 1]
	projTex.x = 0.5f * projTex.x + 0.5f;
	projTex.y = -0.5f * projTex.y + 0.5f;

	float dx = 1.0f / SMAP_SIZE;
	float2 texelPos = SMAP_SIZE * projTex.xy;
	float2 lerps = frac(texelPos);

	float realDepth = input.shadowScreenPosition.z / gShadowFarPlane;
	float maxDepth = samplerShadowMap(projTex.xy);
	float s0 = (samplerShadowMap(projTex.xy) + ShadowBias < realDepth) ? 0.0f : 1.0f;

	float2 projTexXY = projTex.xy;
	float s1 = (samplerShadowMap(projTexXY + float2(dx, 0.0f)) + ShadowBias < realDepth) ? 0.0f : 1.0f;
	float s2 = (samplerShadowMap(projTexXY + float2(0.0f, dx)) + ShadowBias < realDepth) ? 0.0f : 1.0f;
	float s3 = (samplerShadowMap(projTexXY + float2(dx, dx)) + ShadowBias < realDepth) ? 0.0f : 1.0f;

	float lerp0 = lerp(s0, s1, lerps.x);
	float lerp1 = lerp(s2, s3, lerps.x);

	float shadow = lerp(lerp0,
						lerp1,
						lerps.y);
	if (gUseShadowLerp == false)
	{
		shadow = s0;
	}
	
	if (projTex.z < 0)
	{
		shadow = 0.0f;
	}

	float light = dot(normalize(input.Normal), normalize(gDirToSunW));
	light = clamp(light + 0.4f, 0, 1); 

	float3 detail;
	if ( gUseNoise == 1 )
	{	
		detail = tex2D(gNoiseSampler, input.UV * gNoiseScale);
		float detailAmt = input.Depth / gNoiseDistance;
		detail = lerp(detail, 1, clamp(detailAmt, 0, 1));
	}
	else if (gUseNoise == 2)
	{
		detail = float3(1,1,1);
	}
	else
	{
		detail = tex2D(gNoiseSampler, input.UV * gNoiseScale);
	}
	float fogLerpParam = 0;
	if (gFogEnabled)
		fogLerpParam = saturate((input.Depth - gFogStart) / gFogRange);
	float3 tex = tex2D(gTex0Sampler, input.UV * gTexScale);
	float3 texColor = detail * tex * light * shadow;
	float3 final = lerp(texColor, gFogColor, fogLerpParam);

	return float4(final, 1);
}


technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_2_0 VertexShaderFunction();
		pixelShader = compile ps_3_0 PixelShaderFunction();
	}
};