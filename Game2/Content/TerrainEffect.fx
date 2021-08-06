uniform extern float4x4 gViewProj;
uniform extern float3 gPosCamera;
uniform extern float3 gDirToSunW;
uniform extern texture2D gTex0;
uniform extern float gTexScale;
uniform extern Texture2D gNoise;
uniform extern float gNoiseScale;
uniform extern float gNoiseDistance;
extern int gUseNoise = 0;


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
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = mul(input.Position, gViewProj);
	output.Normal = input.Normal;
	output.UV = input.UV;
	float3 vec = gPosCamera - input.Position;
	output.Depth = sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
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


	float3 tex = tex2D(gTex0Sampler, input.UV * gTexScale);

	return float4(detail * tex * light, 1);
}


technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_2_0 VertexShaderFunction();
		pixelShader = compile ps_2_0 PixelShaderFunction();
	}
};