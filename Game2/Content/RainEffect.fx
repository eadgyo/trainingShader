uniform extern float4x4 gWVP;

// Particle texture
uniform extern texture gTex;

// The position of the camera in the local space
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
	MipFilter = POINT;
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

	float mySize = 0.0035f * gViewportHeight * size;

	float3 vec = gEyePosL - posL;

	// Compute billboard matrix
	float3 look = normalize(vec);
	float3 right = normalize(cross(float3(0.0f, 1.0f, 0.0f), look));
	float3 up = normalize(cross(look, right));

	float2 centeredTex = tex - float2(0.5f, 0.5f);
	float3 texVec = -centeredTex.x * right + -centeredTex.y * up;
	posL += texVec * mySize;

	// Constant acceleration
	posL = posL + vel * t +  gAccel * t * t * 0.5f;

	// Transform to homogeneous clip space
	outVS.posH = mul(float4(posL, 1.0f), gWVP);
	outVS.time = lifeTime;

	// Also compute size as a function of the distance from the amera, and the viewport height.
	// The constants were found by experimenting.
	float d = distance(posL, gEyePosL);

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
	float4 myColor = tex2D(TexS, tex0);
	if (time < 0.0f || myColor.a < 0.1f)
	{
		discard;
	}

	return myColor;
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_2_0 FireRingVS();
		pixelShader = compile ps_2_0 FireRingPS();
	}
};