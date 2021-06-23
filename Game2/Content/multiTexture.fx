uniform extern Texture2D gTex0;
uniform extern Texture2D gTex1;
uniform extern Texture2D blendMap;


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

sampler blendLin = sampler_state
{
	Texture = <blendMap>;
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
	float2 UV_notTiled : TEXCOORD1;
};


OutputVS TransformVS(float3 posL : POSITION0, float3 normalL : NORMAL0, float2 UV : TEXCOORD0)
{
	OutputVS outVS = (OutputVS)0;
	outVS.posH = mul(float4(posL, 1.0f), gWVP);
	outVS.UV = UV;
	outVS.UV_notTiled = UV;

	return outVS;
}

float4 TransformPS(float2 UV : TEXCOORD0, float2 UV_notTiled : TEXCOORD1) : COLOR
{
	float3 texColor = tex2D(samLinear, UV).rgb;
	float3 texColor2 = tex2D(second, UV).rgb;

	float3 B = tex2D(blendLin, UV_notTiled).rgb;
	float3 B2 = tex2D(blendLin, UV_notTiled).rgb;

	float totalInverse = 1.0f / (B.g + B2.r);


	float4 color = float4(texColor * B.g * totalInverse + texColor2 * B2.r * totalInverse, 1.0f);

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