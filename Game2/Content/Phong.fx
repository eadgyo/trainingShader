uniform extern float4x4 gWorldInverseTranspose;
uniform extern float4x4 World;
uniform extern float4x4 View;
uniform extern float4x4 Projection;
uniform extern float4x4 gWVP;

uniform extern float4 gAmbientMtrl;
uniform extern float4 gAmbientLight;
uniform extern float4 gDiffuseMtrl;
uniform extern float4 gDiffuseLight;
uniform extern float3 gLightVecW;

uniform extern float4 gSpecularMtrl;
uniform extern float4 gSpecularLight;
uniform extern float gSpecularPower;

uniform extern float3 gEyePos;
uniform extern bool TextureEnabled = true;

struct OutputVS
{
	float4 posH : POSITION0;
	float4 normalW : TEXCOORD0;
	float4 posW : TEXCOORD1;
};

OutputVS PhongVS(float3 posL : POSITION0, float3 normalL : NORMAL0)
{
	OutputVS outVS = (OutputVS)0;

	outVS.normalW = mul(float4(normalL, 0.0f), gWorldInverseTranspose);
	outVS.normalW = normalize(outVS.normalW);

	outVS.posW = mul(float4(posL, 1.0f), World);

	float4x4 wvp = mul(World, mul(View, Projection));

	outVS.posH = mul(float4(posL, 1.0f), gWVP);

	return outVS;
}


float4 PhongPS(OutputVS input) : COLOR
{
	float3 toEye = normalize(gEyePos - input.posW.xyz);

	float r = reflect(-gLightVecW, input.normalW.xyz);

	float t = pow(max(dot(r, toEye), 0.0f), gSpecularPower);

	float s = max(dot(gLightVecW, input.normalW.xyz), 0.0f);

	float3 spec = t * (gSpecularMtrl * gSpecularLight).rgb;
	float3 diffuse = s * (gDiffuseMtrl * gDiffuseLight).rgb;
	float3 ambient = gAmbientMtrl * gAmbientLight;
	
	float4 color = float4(0, 0, 0, 1.00);
	color.rgb = (ambient + diffuse + spec).rgb;
	color.a = gDiffuseMtrl.a;

	return color;
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_4_0 PhongVS();
		pixelShader = compile ps_4_0 PhongPS();
	}
};