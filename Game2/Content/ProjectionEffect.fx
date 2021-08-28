uniform extern float4x4 gWorld;
uniform extern float4x4 gWorldInverseTranspose;
uniform extern float4x4 gLightWVP;
uniform extern float4x4 gWVP;
uniform extern float3 gSunDirection;
uniform extern float3 gEyePos;
uniform extern texture gTex;

sampler TexS = sampler_state
{
	Texture = <gTex>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

struct OutputVS
{
	float4 posH	: POSITION0;
	float4 posW : TEXCOORD0;
	float4 normalW : TEXCOORD1;
	float4 toEyeW : TEXCOORD2;
	float4 projTex : TEXCOORD3;
};

OutputVS TransformVS(float3 posL : POSITION0, float3 normalL : NORMAL0)
{
	// Zero out our output
	OutputVS outVS = (OutputVS)0;

	// Transform normal to world space
	outVS.normalW = mul(float4(normalL, 0.0f), gWorldInverseTranspose);

	// Transform vertex position to world space
	outVS.posW = mul(float4(posL, 1.0f), gWorld);

	// Compute the unit vector from the vertex to the eye
	outVS.toEyeW = float4(gEyePos,1.0f) - outVS.posW;

	// Transform to homogeneous clip space
	outVS.posH = mul(float4(posL, 1.0f), gWVP);

	// Render from light WVP
	outVS.projTex = mul(outVS.posW, gLightWVP);

	return outVS;
}


float4 TransformPS(OutputVS input) : COLOR
{
	// Interpolated normals can become unnormal
	input.normalW = normalize(input.normalW);
	input.toEyeW = normalize(input.toEyeW);
	/*
	// Light vector is from pixel to spotlight position
	float3 lightVecW = normalize(gSunPositionW- posW.xyz);
	
	// Compute the reflection vector
	float3 r = reflec(-lightVecW, normalW.xyz);

	// Determine how much (if any) specular light makes it into the eye
	float t = dot(r, toEyeW);*/

	// Project the texture coords and scale/offset to [0, 1]
	input.projTex.xy /= input.projTex.w;
	input.projTex.x = 0.5f * input.projTex.x + 0.5f;
	input.projTex.y = -0.5f * input.projTex.y + 0.5f;

	// Sample tex w/ projective texture coords.
	float4 texColor = tex2D(TexS, input.projTex.xy);
	/*
	if (texColor.a < 0.2f)
		discard;*/

	return texColor;
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_4_0 TransformVS();
		pixelShader = compile ps_4_0 TransformPS();
	}
};