uniform extern texture gRadarTexture;
uniform extern float4x4 gWVP;
uniform extern float gViewportWidth;
uniform extern float gViewportHeight;

struct OutputVS
{
	float4 posH : POSITION0;
	float2 UV : TEXCOORD0;
};

sampler2D radarSampler = sampler_state
{
	texture = <gRadarTexture>;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	AddressU = mirror;
	AddressV = mirror;
};

OutputVS TransformVS(float3 posL : POSITION0, float2 tex : TEXCOORD0)
{
	// Zero out our output
	OutputVS outVS = (OutputVS)0;

	// Transform to homogeneous clip space
	outVS.posH = mul(float4(posL, 1.0f), gWVP);
	//outVS.posH = float4(posL.x / gViewportWidth, posL.y / gViewportHeight, posH.z, posH.w);
	//outVS.posH = mul(float4(posL, 1.0f), gWVP);
	outVS.UV = tex;
	return outVS;
}

float4 TransformPS(OutputVS input) : COLOR
{
	float4 texColor = tex2D(radarSampler, input.UV);

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