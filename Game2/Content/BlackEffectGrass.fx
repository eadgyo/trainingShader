uniform extern float4x4 gWVP;
extern float4 myColor;
uniform extern texture2D gTex;
uniform extern float3 gEyePosW;

struct VertexShaderInput
{
	float3 posL : POSITION0;
	float3 quadPosW : TEXCOORD0;
	float2 tex0 : TEXCOORD1;
};

struct OutputVS
{
	float4 posH : POSITION0;
	float2 tex0 : TEXCOORD0;
};

sampler2D TexS = sampler_state
{
	Texture = <gTex>;
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = POINT;
	AddressU = WRAP;
	AddressV = WRAP;
};


OutputVS TransformVS(VertexShaderInput input)
{
	// Zero out our output
	OutputVS outVS;

	// Compute billboard matrix
	float3 look = normalize(gEyePosW - input.quadPosW);
	float3 right = normalize(cross(float3(0.0f, 1.0f, 0.0f), look));
	float3 up = normalize(cross(look, right));

	// Build look-at rotation matrix that makes the
	// billboard face the camera
	float4x4 lookAtMtx;
	lookAtMtx[0] = float4(right, 0.0f);
	lookAtMtx[1] = float4(up, 0.0f);
	lookAtMtx[2] = float4(look, 0.0f);
	lookAtMtx[3] = float4(input.quadPosW, 1.0f);

	// Transform to worldSpace
	float4 posW = mul(float4(input.posL, 1.0f), lookAtMtx);

	// Transform to homogeneous clip space
	outVS.posH = mul(posW, gWVP);
	outVS.tex0 = input.tex0;
	return outVS;
}

float4 TransformPS(OutputVS input) : COLOR0
{
	float4 texColor = tex2D(TexS, input.tex0);

	return texColor;
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_4_0 TransformVS();
		pixelShader = compile ps_4_0 TransformPS();
	}
};