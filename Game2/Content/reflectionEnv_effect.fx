uniform extern float4x4 gWVP;
uniform extern texture gCubeMap;
uniform extern float3 gCameraPosition;
uniform extern float4x4 gWorld;
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
	float4 posW : TEXCOORD0;
	float4 normal : TEXCOORD1;
};

OutputVS TransformVS(float3 posL : POSITION0, float3 normal : NORMAL0)
{
	// Zero out our output
	OutputVS outVS = (OutputVS)0;

	// Transform to homogeneous clip space
	outVS.posH = mul(float4(posL, 1.0f), gWVP);
	outVS.normal = mul(float4(normal, 0.0f), gWorld);
	outVS.posW = mul(float4(posL, 1.0f), gWorld);
	return outVS;
}

float4 TransformPS(float4 posW : TEXCOORD0, float4 normal : TEXCOORD1) : COLOR
{
	if (gClipPlaneEnabled)
		clip(dot(posW, gClipPlane));


	float3 viewDirection = normalize(posW.xyz - gCameraPosition);
	float3 normalN = normalize(normal.xyz);

	float3 reflection = reflect(viewDirection, normalN);

	return texCUBE(CubeMapSampler, reflection);
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_2_0 TransformVS();
		pixelShader = compile ps_2_0 TransformPS();
	}
};