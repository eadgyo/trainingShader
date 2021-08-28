uniform extern float4x4 gViewProj;
uniform extern float3 gPosCamera;
uniform extern float3 gDirToSunW;
uniform extern texture2D gTex0;
uniform extern float gTexScale;
uniform extern Texture2D gNoise;
uniform extern float gNoiseScale;
uniform extern float gNoiseDistance;
extern int gUseNoise = 0;

static float3 gFogColor = { 0.5f, 0.5f, 0.5f };
static float gFogStart = 1.0f;
static float gFogRange = 5000.0f;
uniform extern bool gFogEnabled;

uniform extern bool gClipPlaneEnabled = true;
uniform extern float4 gClipPlane;


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
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	if (gClipPlaneEnabled)
		clip(dot(input.posW, gClipPlane));

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
	float3 texColor = detail * tex * light;
	float3 final = lerp(texColor, gFogColor, fogLerpParam);

	return float4(final, 1);
}


technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_4_0 VertexShaderFunction();
		pixelShader = compile ps_4_0 PixelShaderFunction();
	}
};