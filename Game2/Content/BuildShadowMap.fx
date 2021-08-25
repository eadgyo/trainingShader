uniform extern float4x4 gLightWVP;

void TransformVS(float3 posL : POSITION0,
				out float4 posH : POSITION0,
				out float2 depth : TEXCOORD0)
{
	
	// Transform to homogeneous clip space
	posH = mul(float4(posL, 1.0f), gLightWVP);

	depth = posH.zw;
}

float4 TransformPS(float2 depth : TEXCOORD0) : COLOR
{
	// Each pixel in the shadow map stores the pixel depth from the light 
	// source in normalized device coordinates
	return depth.x / depth.y; // Z / W; deph in [0, 1] range.
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_2_0 TransformVS();
		pixelShader = compile ps_2_0 TransformPS();
	}
};