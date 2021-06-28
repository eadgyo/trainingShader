uniform extern float4x4 gWVP;
extern float4 myColor;

struct OutputVS
{
	float4 posH : POSITION0;
};

OutputVS TransformVS(float3 posL : POSITION0)
{
	// Zero out our output
	OutputVS outVS = (OutputVS)0;

	// Transform to homogeneous clip space
	outVS.posH = mul(float4(posL, 1.0f), gWVP);

	return outVS;
}

float4 TransformPS() : COLOR
{
	return myColor;
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_2_0 TransformVS();
		pixelShader = compile ps_2_0 TransformPS();
	}
};