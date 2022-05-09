float4 _CTI_SRP_Wind;
float  _CTI_SRP_Turbulence;

float3x3 GetRotationMatrix(float3 axis, float angle)
{
	//axis = normalize(axis); // moved to calling function
	float s = sin(angle);
	float c = cos(angle);
	float oc = 1.0 - c;

	return float3x3	(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s,
		oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s,
		oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c);
}

inline float4 SmoothCurve(float4 x) {
	return x * x *(3.0 - 2.0 * x);
}

inline float4 TriangleWave(float4 x) {
	return abs(frac(x + 0.5) * 2.0 - 1.0);
}

inline float4 SmoothTriangleWave(float4 x) {
	return SmoothCurve(TriangleWave(x));
}

inline float3 FastSign(float3 x) {
	#define FLT_MAX 3.402823466e+38 // Maximum representable floating-point number
	return saturate(x * FLT_MAX + 0.5) * 2.0 - 1.0;
}

// This function always return the absolute position in WS
float3 CTI_GetAbsolutePositionWS(float3 positionRWS)
{
#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
    positionRWS += _WorldSpaceCameraPos;
#endif
    return positionRWS;
}
// Return absolute world position of current object
float3 CTI_GetObjectAbsolutePositionWS()
{
    float4x4 modelMatrix = UNITY_MATRIX_M;
    return CTI_GetAbsolutePositionWS(modelMatrix._m03_m13_m23); // Translation object to world
}

// we need that precision suffix "_float"
void ApplyCTIWind_float(
	in float3   position,
	in float4	vertexColor,
	in float2	uv1,
	in float2 	uv2,
	in float3 	normal,

	in float 	tumbleStrength,
	in float    tumbleFrequency,
	in float 	leafTurbulence,
	in float 	leafNoise,
	in float3   windMultipliers,

	in float3 	timeParams,

	in bool 	isBark,

	out float3 	positionOut,
	out float3  normalOut,
	out float   colorOut
) {
	const float fDetailAmp = 0.1f;
	const float fBranchAmp = 0.3f;
	#define Phase vertexColor.r
	#define Flutter vertexColor.g
	#define BranchBending uv1.y
	#define MainBending uv1.x

	float turbulence = _CTI_SRP_Turbulence;
	float3 windDir = mul((float3x3)GetWorldToObjectMatrix(), _CTI_SRP_Wind.xyz); // TransformWorldToObjectDir(_TerrainLODWind.xyz); // from VertMesh.hlsl
	float absWindStrength = _CTI_SRP_Wind.w;
	float4 Wind = float4(windDir, absWindStrength);

//  unity_ObjectToWorld does not work... but at least we can use the functions from HDRP!
	const float3 TreeWorldPos = CTI_GetObjectAbsolutePositionWS();

		//float sinuswave = _SinTime.z;
		//float shiftedsinuswave = (_SinTime.z + _SinTime.w) * 0.5;
	float sinuswave = sin(timeParams.x * 0.5f);
	float shiftedsinuswave = (sinuswave + timeParams.y) * 0.5;

	float4 vOscillations = SmoothTriangleWave(float4(TreeWorldPos.x + sinuswave, TreeWorldPos.z + sinuswave * 0.7, TreeWorldPos.x + shiftedsinuswave, TreeWorldPos.z + shiftedsinuswave * 0.8));
	// x used for main wind bending / y used for tumbling
	float2 fOsc = vOscillations.xz + (vOscillations.yw * vOscillations.yw);
	fOsc = 0.75 + (fOsc + 3.33) * 0.33;


	float fObjPhase = abs(frac((TreeWorldPos.x + TreeWorldPos.z) * 0.5) * 2 - 1);
	float fBranchPhase = fObjPhase + Phase;
	float fVtxPhase = dot(position, Flutter + fBranchPhase);

	// x is used for edge fluttering / y is used for branch bending
		// float2 vWavesIn = _Time.yy + float2(fVtxPhase, fBranchPhase);
	float2 vWavesIn = timeParams.xx + float2(fVtxPhase, fBranchPhase);

	// 1.975, 0.793, 0.375, 0.193 are good frequencies
	float4 vWaves = (frac(vWavesIn.xxyy * float4(1.975, 0.793, 0.375, 0.193)) * 2.0 - 1.0);
	vWaves = SmoothTriangleWave(vWaves);
	float2 vWavesSum = vWaves.xz + vWaves.yw;

//	Leaf Tumbling
//  isBark gets set at compile time. So the shader compiler should strip this out.
	if (!isBark) {
		
		float3 pivot;
		//#if defined(LEAFTUMBLING)
		// 15bit compression 2 components only, important: sign of y
		pivot.xz = (frac(float2(1.0f, 32768.0f) * uv2.xx) * 2) - 1;
		pivot.y = sqrt(1 - saturate(dot(pivot.xz, pivot.xz)));
		pivot *= uv2.y;

		float tumbleInfluence = frac(vertexColor.b * 2.0);
		// Move point to 0,0,0
		position -= pivot;
			
		float3 fracs = frac(pivot * 33.3);

		float offset = fracs.x + fracs.y + fracs.z	/* this adds a lot of noise, so we use * 0.1 */ + (BranchBending  + Phase) * leafNoise;
		float tFrequency = tumbleFrequency * (_Time.y /* new */ + fObjPhase * 10);
		float4 vWaves1 = SmoothTriangleWave(float4((tFrequency + offset) * (1.0 + offset * 0.25), tFrequency * 0.75 + offset, tFrequency * 0.5 + offset, tFrequency * 1.5 + offset));

		// we could do better in case we have the baked branch main axis
		// float facingWind = dot(normalize(float3(position.x, 0, position.z)), windDir);

		float3 windTangent = float3(-windDir.z, windDir.y, windDir.x);
		float twigPhase = vWaves1.x + vWaves1.y + (vWaves1.z * vWaves1.z);
		//float windStrength = dot(abs(Wind.xyz), 1) * tumbleInfluence * (1.35 - facingWind) * Wind.w + absWindStrength; // Use abs(_Wind)!!!!!!
		float3 branchAxis = cross(windTangent, float3(-1, 0, 1));

	//	it makes much sense to include BranchBending here
		UNITY_BRANCH if (leafTurbulence != 0.0f) {
			float angle =
				// center rotation so the leaves rotate leftwards as well as rightwards according to the incoming waves
				((twigPhase + vWaves1.w) * 0.25 - 0.5   +    BranchBending    )
				// make rotation strength depend on absWindStrength and all other inputs
				* absWindStrength * leafTurbulence * tumbleInfluence * fOsc.x
				;
			float3x3 turbulenceRot = GetRotationMatrix(-branchAxis, angle);
			position = mul(turbulenceRot, position);
			normal = mul(turbulenceRot, normal);
		}

		UNITY_BRANCH if(tumbleStrength != 0.0f) {
			float angleTumble = (absWindStrength * (twigPhase + fBranchPhase * 0.25      + BranchBending    ) * tumbleStrength * tumbleInfluence * fOsc.y);
			float3x3 tumbleRot = GetRotationMatrix(windTangent, angleTumble);
			position = mul(tumbleRot, position);
			normal = mul(tumbleRot, normal);
		}

	//	fade in/out leave planes
			// float lodfade = ceil(pos.w - 0.51);
	//  asset store
			// float lodfade = (pos.w > 0.5) ? 1 : 0;
	//  latest
		UNITY_BRANCH if (unity_LODFade.x < 1.0) {
			float lodfade = (vertexColor.b > (1.0f/255.0f*126.0f) ) ? 1 : 0; // Make sure that the 1st vertex is taken into account
			position.xyz *= 1.0 - unity_LODFade.x * lodfade;
		}
	//	Move point back to origin
		position.xyz += pivot;
	}

//	Preserve Length
	float origLength = length(position);

//	Secondary bending and edge flutter
	float3 bend = Flutter * fDetailAmp * normal * FastSign(normal) * windMultipliers.z;
	bend.y = BranchBending * fBranchAmp * windMultipliers.y;
	position += (((vWavesSum.xyx * bend  *  absWindStrength ) + (vWavesSum.y * BranchBending * turbulence * windMultipliers.y )));

//	Primary bending / Displace position
	position += MainBending * Wind.xyz * fOsc.x * absWindStrength  * windMultipliers.x;
//	Preserve Length
	position = normalize(position) * origLength;
//	Copy to output
	positionOut = position;
	normalOut = normalize(normal);
//	Store Variation
	colorOut = saturate ( ( frac(TreeWorldPos.x + TreeWorldPos.y + TreeWorldPos.z) + frac( (TreeWorldPos.x + TreeWorldPos.y + TreeWorldPos.z) * 3.3 ) ) * 0.5 );
}