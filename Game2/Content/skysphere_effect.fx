uniform extern float4x4 gWVP;
uniform extern texture gCubeMap;
uniform extern bool gClipPlaneEnabled = true;
uniform extern float4 gClipPlane;


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

float4 TransformPS(OutputVS input) : COLOR
{
	if (gClipPlaneEnabled)
		clip(dot(float4(input.posL, 1), gClipPlane));

	return texCUBE(CubeMapSampler, input.posL);
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_4_0 TransformVS();
		pixelShader = compile ps_4_0 TransformPS();
	}
};