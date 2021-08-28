uniform extern float4x4 gWVP;

// Particle texture
uniform extern texture gTex;

// The poisition of the camera in the local space
uniform extern float3 gEyePosL;

// Constant acceleration vector
uniform extern float3 gAccel;

// Particle system time -- corresponds to PSystem::mTime
uniform extern float gTime;

// Viewport height for scaling the point sprite sizes; see comment in PSystem::draw
uniform extern float gViewportHeight;

sampler TexS = sampler_state
{
	Texture = <gTex>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

struct OutputVS
{
	float4 posH : POSITION0;
	float4 color : COLOR0;
	float2 tex0 : TEXCOORD0;
	float time : TEXCOORD1;
};

OutputVS FireRingVS(float3 posL		: POSITION0,
					float2 tex		: TEXCOORD0,
					float3 vel		: TEXCOORD1,
					float size		: TEXCOORD2,
					float time		: TEXCOORD3,
					float lifeTime	: TEXCOORD4,
					float mass		: TEXCOORD5,
					float4 color	: COLOR0)
{
	// Zero out our output
	OutputVS outVS = (OutputVS)0;



	// Get age of particle from cration time
	float t = gTime - time;

	// Also compute size as a function of the distance from the amera, and the viewport height.
	// The constants were found by experimenting.
	float d = distance(posL, gEyePosL);

	// Decrease size over time to simulate the flare diminution
	// over time. Formula found by experimenting.
	float divide = clamp((1+t)/2, 1, 200);
	size = size / divide;
	size = gViewportHeight * size / (1.0f * 8.0f * d);
	float2 centeredTex = tex - float2(0.5f, 0.5f);
	posL += (float3(centeredTex, 0))* size;

	// Constant acceleration
	posL = posL + vel * t +  gAccel * t * t * 0.5f;

	// Transform to homogeneous clip space
	outVS.posH = mul(float4(posL, 1.0f), gWVP);
	outVS.time = lifeTime;

	// Fade color from white to black over the particle's lifetime
	// to fade it out gradually
	outVS.color = (1.0f - (t / lifeTime));
	outVS.tex0 = tex;

	return outVS;
}

float4 FireRingPS(OutputVS input) : COLOR
{
	float4 myColor = tex2D(TexS, input.tex0);
	if (input.time < 0.0f || myColor.a < 0.1f)
	{
		discard;
	}

	return myColor;
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_4_0 FireRingVS();
		pixelShader = compile ps_4_0 FireRingPS();
	}
};