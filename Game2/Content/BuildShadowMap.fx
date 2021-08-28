uniform extern float4x4 gLightWVP;
uniform extern float4x4 gWorld;
uniform extern float gFarPlane;

void TransformVS(float3 posL : POSITION0,
				out float4 posH : POSITION0,
				out float4 screenPos : TEXCOORD0)
{
	float4 posW = mul(float4(posL, 1.0f), gWorld);

	// Transform to homogeneous clip space
	posH = mul(posW, gLightWVP);

	screenPos = posH;
}

float4 TransformPS(float4 screenPos : TEXCOORD0) : COLOR
{
	// Each pixel in the shadow map stores the pixel depth from the light 
	// source in normalized device coordinates
	float depth = clamp(screenPos.z / gFarPlane, 0.0f, 1.0f);

	return depth; // Z / W; deph in [0, 1] range.
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_4_0 TransformVS();
		pixelShader = compile ps_4_0 TransformPS();
	}
};