uniform extern texture gTex;
uniform extern float4x4 gViewProj;
uniform extern float gTime;
uniform extern float3 gDirToSunW;
uniform extern float3 gEyePosW;

uniform extern bool gUseAlpha = false;
uniform extern float gReferenceAlpha = 0.5f;
uniform extern bool gAlphaTestGreater = true;

static float3 gFogColor = { 0.5f, 0.5f, 0.5f };
static float gFogStart = 1.0f;
static float gFogRange = 5000.0f;

sampler TexS = sampler_state
{
	Texture = <gTex>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

struct OutputVS
{
	float4 posH : POSITION0;
	float2 tex0 : TEXCOORD0;
	float Depth : TEXCOORD1;
	float4 colorOffset : COLOR0;
};

OutputVS TransformVS(float3 posL : POSITION0,
					float3 quadPosW : TEXCOORD0,
					float2 tex0 : TEXCOORD1,
					float amplitude : TEXCOORD2,
					float4 colorOffset : COLOR0)
{
	// Zero out our output
	OutputVS outVS = (OutputVS)0;

	// Compute billboard matrix
	float3 look = normalize(gEyePosW - quadPosW);
	float3 right = normalize(cross(float3(0.0f, 1.0f, 0.0f), look));
	float3 up = normalize(cross(look, right));
	
	// Build look-at rotation matrix that makes the
	// billboard face the camera
	float4x4 lookAtMtx;
	lookAtMtx[0] = float4(right, 0.0f);
	lookAtMtx[1] = float4(up, 0.0f);
	lookAtMtx[2] = float4(look, 0.0f);
	lookAtMtx[3] = float4(quadPosW, 1.0f);

	// Transform to worldSpace
	float4 posW = mul(float4(posL, 1.0f), lookAtMtx);
	
	// Oscillate the vertices based on their mamplitude factor.
	// Only oscillate top vertices.
	float sine = amplitude * sin(amplitude * gTime);

	// Oscillate along right vector.
	posW.xyz += sine * right;

	// Oscillate the color offset
	outVS.colorOffset.r = colorOffset + 0.1f * sine;
	outVS.colorOffset.g = colorOffset + 0.2f * sine;
	outVS.colorOffset.b = colorOffset + 0.1f * sine;

	outVS.posH = mul(posW, gViewProj);
	outVS.tex0 = tex0;
	float3 vec = gEyePosW - posW;
	outVS.Depth = sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
	outVS.colorOffset = colorOffset;

	return outVS;
}

float4 TransformPS(float2 tex0 : TEXCOORD0,
				float depth : TEXCOORD1,
				float4 colorOffset : COLOR0) : COLOR
{
	float fogLerpParam = saturate((depth - gFogStart) / gFogRange);
	float4 texColor = tex2D(TexS, tex0);
	texColor += colorOffset;
	float3 final = lerp(texColor.rgb, gFogColor, fogLerpParam);
	if (gUseAlpha == true)
	{
		// Discard if below 0
		clip((texColor.a - gReferenceAlpha)* (gAlphaTestGreater ? 1 : -1));
	}

	return float4(final, texColor.a);
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_2_0 TransformVS();
		pixelShader = compile ps_2_0 TransformPS();
	}
};