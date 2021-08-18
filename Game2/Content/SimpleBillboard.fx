uniform extern texture gTex;
uniform extern float4x4 gWVP;

sampler TexS = sampler_state
{
	Texture = <gTex>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

struct OutputVS
{
	float4 posH : POSITION0;
	float2 tex0 : TEXCOORD0;
};

OutputVS TransformVS(float3 posL : POSITION0,
					float2 tex0 : TEXCOORD0)
{
	// Zero out our output
	OutputVS outVS = (OutputVS)0;
	outVS.posH = mul(float4(posL, 1.0f), gWVP);
	outVS.tex0 = tex0;

	return outVS;
}

float4 TransformPS(float2 tex0 : TEXCOORD0) : COLOR
{
	float4 texColor = tex2D(TexS, tex0);
	return float4(texColor.rgb, texColor.a);
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_2_0 TransformVS();
		pixelShader = compile ps_2_0 TransformPS();
	}
};