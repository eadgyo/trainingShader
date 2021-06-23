uniform extern Texture2D gTex0;
uniform extern Texture2D gTex1;


uniform extern float4x4 gWVP;


sampler samLinear = sampler_state
{
	Texture = <gTex0>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler second = sampler_state
{
	Texture = <gTex1>;
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
	outVS.UV = UV;

	return outVS;
}

float4 TransformPS(float2 UV : TEXCOORD0) : COLOR
{
	float3 texColor = tex2D(samLinear, UV).rgb;
	float3 texColor2 = tex2D(second, UV).rgb;

	float4 color = float4(texColor * 0.5f + texColor2 * 0.5f, 1.0f);

	return color;
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_3_0 TransformVS();
		pixelShader = compile ps_3_0 TransformPS();
	}
};