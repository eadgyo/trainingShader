uniform extern float4x4 gWVP;
uniform extern texture gCubeMap;

samplerCUBE CubeMapSampler = sampler_state {
	texture = <gCubeMap>;
	minFilter = anisotropic;
	magfilter = anisotropic;
};

struct OutputVS
{
	float4 posH : POSITION0;
	float3 posL : TEXCOORD1;
};

OutputVS TransformVS(float3 posL : POSITION0)
{
	// Zero out our output
	OutputVS outVS = (OutputVS)0;

	// Transform to homogeneous clip space
	outVS.posH = mul(float4(posL, 1.0f), gWVP).xyww;
	outVS.posL = posL;
	return outVS;
}

float4 TransformPS(float3 posL : TEXCOORD1) : COLOR
{
	return texCUBE(CubeMapSampler, posL);
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_2_0 TransformVS();
		pixelShader = compile ps_2_0 TransformPS();
	}
};