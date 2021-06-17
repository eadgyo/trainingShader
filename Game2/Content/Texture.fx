
struct OutputVS
{
	float4 posH : POSITION0;
	float4 color : COLOR0;
};

OutputVS TransformVS(float3 posL : POSITION0, float3 normalL : NORMAL0)
{
	OutputVS outVS = (OutputVS)0;


	return outVS;
}

float4 TransformPS(float4 color : COLOR0) : COLOR
{
	return color;
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_2_0 TransformVS();
		pixelShader = compile ps_2_0 TransformPS();
	}
};