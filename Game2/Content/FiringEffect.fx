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

	// Rotate the particles about local space about z-axis as a
	// function of time. These are just the rotation equations.
	/*float sine, cosine;
	sincos(0.5f * mass * t, sine, cosine);
	float x = posL.x * cosine - posL.y * sine;
	float y = posL.x * sine + posL * cosine;
	
	// Oscillate particles up and down
	float s = sin(6.0f * t);
	posL.x = x;
	posL.y = y + mass*s;*/

	// Constant acceleration
	posL = posL + vel * t +  gAccel * t * t * 0.5f;

	// Transform to homogeneous clip space
	outVS.posH = mul(float4(posL, 1.0f), gWVP);
	outVS.time = lifeTime;

	// Ramp up size over time to simulate the flare expanding
	// over time. Formula found by experimenting.
	size += 8.0f * t * t;

	// Also compute size as a function of the distance from the amera, and the viewport height.
	// The constants were found by experimenting.
	float d = distance(posL, gEyePosL);
	//outVS.size = gViewportHeight * size / (1.0f * 8.0f * d);

	// Fade color from white to black over the particle's lifetime
	// to fade it out gradually
	outVS.color = (1.0f - (t / lifeTime));
	outVS.tex0 = tex;

	return outVS;
}

float4 FireRingPS(float4 color : COLOR0,
				float2 tex0 : TEXCOORD0,
				float time : TEXCOORD1) : COLOR
{
	if (time < 0.0f)
	{
		discard;
	}

	return tex2D(TexS, tex0);
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_2_0 FireRingVS();
		pixelShader = compile ps_2_0 FireRingPS();
	}
};