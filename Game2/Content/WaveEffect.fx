uniform extern float4x4 gWVP;

// Amplitudes
static float a[2] = { 0.8f, 0.2f };

// Angular wave numbers
static float k[2] = { 1.0, 8.0f };

// Angular frequency
static float w[2] = { 1.0f, 8.0f };

// Phase shift
static float p[2] = { 0.0f, 1.0f };

uniform extern float gTime;

float SumOfRadialSineWaves(float y, float z)
{
	// Distance of vertex from source of waves
	float d = sqrt(y * y + z * z);

	// Sum of the waves
	float sum = 0.0f;
	for (int i = 0; i < 2; ++i)
	{
		sum += a[i] * sin(k[i] * d - gTime * w[i] + p[i]);
	}

	return sum;
}

float4 GetColorFromHeight(float y)
{
	if (abs(y) <= 0.2f)
		return float4(0.0f, 0.0f, 0.0f, 1.0f);
	else if (abs(y) <= 0.4f)
		return float4(0.0f, 0.0f, 1.0f, 1.0f);
	else if (abs(y) <= 0.6f)
		return float4(0.0f, 1.0f, 0.0f, 1.0f);
	else if (abs(y) <= 0.8f)
		return float4(1.0f, 0.0f, 0.0f, 1.0f);
	else
		return float4(1.0f, 1.0f, 0.0f, 1.0f);
}


struct OutputVS
{
	float4 posH : POSITION0;
	float4 color : COLOR0;
};

OutputVS TransformVS(float3 posL : POSITION0, float4 color : COLOR0)
{
	// Zero out our output
	OutputVS outVS = (OutputVS)0;

	// Get the height of the vertex
	posL.x = SumOfRadialSineWaves(posL.y, posL.z);

	// Get the color based on its height
	outVS.color = GetColorFromHeight(posL.x);

	// Transform to homogeneous clip space
	outVS.posH = mul(float4(posL, 1.0f), gWVP);

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
		vertexShader = compile vs_1_1 TransformVS();
		pixelShader = compile ps_2_0 TransformPS();
	}
};