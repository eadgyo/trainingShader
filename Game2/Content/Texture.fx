uniform extern Texture2D gTex0;

uniform extern float4x4 gWVP;
uniform extern float2 offset;
uniform extern float alpha = 1.0f;


sampler samLinear = sampler_state
{
	Texture = <gTex0>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};
struct OutputVS
{
	float4 posH : POSITION0;
	float2 UV : TEXCOORD0;
};


OutputVS TransformVS(float3 posL : POSITION0, float3 normalL : NORMAL0, float2 UV : TEXCOORD0)
{
	OutputVS outVS = (OutputVS)0;
	outVS.posH = mul(float4(posL, 1.0f), gWVP);
	outVS.UV = UV + offset;

	return outVS;
}

float4 TransformPS(OutputVS input) : COLOR
{
	float3 texColor = tex2D(samLinear, input.UV).rgb;
	float4 color = float4(texColor, alpha);

	return color;
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_4_0 TransformVS();
		pixelShader = compile ps_4_0 TransformPS();
	}
};