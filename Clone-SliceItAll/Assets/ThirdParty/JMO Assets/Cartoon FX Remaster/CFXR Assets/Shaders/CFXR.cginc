//--------------------------------------------------------------------------------------------------------------------------------
// Cartoon FX
// (c) 2012-2020 Jean Moreno
//--------------------------------------------------------------------------------------------------------------------------------

#if defined(GLOBAL_DISABLE_SOFT_PARTICLES) && !defined(DISABLE_SOFT_PARTICLES)
	#define DISABLE_SOFT_PARTICLES
#endif

#if CFXR_URP
	float LinearEyeDepthURP(float depth, float4 zBufferParam)
	{
		return 1.0 / (zBufferParam.z * depth + zBufferParam.w);
	}

	float SoftParticles(float near, float far, float4 projection)
	{
		float sceneZ = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(projection)).r;

	#if defined(SOFT_PARTICLES_ORTHOGRAPHIC)
		// orthographic camera
		#if defined(UNITY_REVERSED_Z)
			sceneZ = 1.0f - sceneZ;
		#endif
		sceneZ = (sceneZ * _ProjectionParams.z) + _ProjectionParams.y;
	#else
		// perspective camera
		sceneZ = LinearEyeDepthURP(sceneZ, _ZBufferParams);
	#endif

		float fade = saturate (far * ((sceneZ - near) - projection.z));
		return fade;
	}
#else
	float SoftParticles(float near, float far, float4 projection)
	{
		float sceneZ = (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(projection)));
	#if defined(SOFT_PARTICLES_ORTHOGRAPHIC)
		// orthographic camera
		#if defined(UNITY_REVERSED_Z)
			sceneZ = 1.0f - sceneZ;
		#endif
		sceneZ = (sceneZ * _ProjectionParams.z) + _ProjectionParams.y;
	#else
		// perspective camera
		sceneZ = LinearEyeDepth(sceneZ);
	#endif

		float fade = saturate (far * ((sceneZ - near) - projection.z));
		return fade;
	}
#endif

		float LinearToGammaSpaceApprox(float value)
		{
			return max(1.055h * pow(value, 0.416666667h) - 0.055h, 0.h);
		}
		
		// Same as UnityStandardUtils.cginc, but without the SHADER_TARGET limitation
		half3 UnpackScaleNormal_CFXR(half4 packednormal, half bumpScale)
		{
			#if defined(UNITY_NO_DXT5nm)
				half3 normal = packednormal.xyz * 2 - 1;
				// #if (SHADER_TARGET >= 30)
					// SM2.0: instruction count limitation
					// SM2.0: normal scaler is not supported
					normal.xy *= bumpScale;
				// #endif
				return normal;
			#else
				// This do the trick
				packednormal.x *= packednormal.w;

				half3 normal;
				normal.xy = (packednormal.xy * 2 - 1);
				// #if (SHADER_TARGET >= 30)
					// SM2.0: instruction count limitation
					// SM2.0: normal scaler is not supported
					normal.xy *= bumpScale;
				// #endif
				normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
				return normal;
			#endif
		}

		//Macros

		// Project Position
	#if !defined(DISABLE_SOFT_PARTICLES) && ( (defined(SOFTPARTICLES_ON) || defined(CFXR_URP) || defined(SOFT_PARTICLES_ORTHOGRAPHIC)) && defined(_FADING_ON) )
		#define vertProjPos(o, clipPos) \
			o.projPos = ComputeScreenPos(clipPos); \
			COMPUTE_EYEDEPTH(o.projPos.z);
	#else
		#define vertProjPos(o, clipPos)
	#endif

		// Soft Particles
	#if !defined(DISABLE_SOFT_PARTICLES) && ((defined(SOFTPARTICLES_ON) || defined(CFXR_URP) || defined(SOFT_PARTICLES_ORTHOGRAPHIC)) && defined(_FADING_ON))
		#define fragSoftParticlesFade(i, color) \
			color *= SoftParticles(_SoftParticlesFadeDistanceNear, _SoftParticlesFadeDistanceFar, i.projPos);
	#else
		#define fragSoftParticlesFade(i, color)
	#endif

		// Edge fade (note: particle meshes are already in world space)
	#if defined(_CFXR_EDGE_FADING)
		#define vertEdgeFade(v, color) \
			float3 viewDir = UnityWorldSpaceViewDir(v.vertex); \
			float ndv = abs(dot(normalize(viewDir), v.normal.xyz)); \
			color *= saturate(pow(ndv, _EdgeFadePow));
	#else
		#define vertEdgeFade(v, color)
	#endif

		// Fog
	#if _ALPHABLEND_ON
		#define applyFog(i, color, alpha)	UNITY_APPLY_FOG_COLOR(i.fogCoord, color, unity_FogColor);
	#elif _ALPHAPREMULTIPLY_ON
		#define applyFog(i, color, alpha)	UNITY_APPLY_FOG_COLOR(i.fogCoord, color, alpha * unity_FogColor);
	#elif _CFXR_ADDITIVE
		#define applyFog(i, color, alpha)	UNITY_APPLY_FOG_COLOR(i.fogCoord, color, half4(0, 0, 0, 0));
	#elif _ALPHAMODULATE_ON
		#define applyFog(i, color, alpha)	UNITY_APPLY_FOG_COLOR(i.fogCoord, color, half4(1, 1, 1, 1));
	#else
		#define applyFog(i, color, alpha)	UNITY_APPLY_FOG_COLOR(i.fogCoord, color, unity_FogColor);
	#endif

		// Vertex program
	#if PASS_SHADOW_CASTER
		void vert(appdata v, v2f_shadowCaster o, out float4 opos)
	#else
		v2f vert(appdata v, v2f o)
	#endif
		{
			UNITY_TRANSFER_FOG(o, o.pos);
			vertProjPos(o, o.pos);
			vertEdgeFade(v, o.color.a);

	#if PASS_SHADOW_CASTER
			TRANSFER_SHADOW_CASTER_NOPOS(o, opos);
	#else
			return o;
	#endif
		}

		// Fragment program
	#if PASS_SHADOW_CASTER
		float4 frag(v2f_shadowCaster i, UNITY_VPOS_TYPE vpos, half3 particleColor, half particleAlpha, half dissolve, half dissolveTime) : SV_Target
	#else
		half4 frag(v2f i, half3 particleColor, half particleAlpha, half dissolve, half dissolveTime) : SV_Target
	#endif
		{
			//Blending
		#if _ALPHAPREMULTIPLY_ON
			particleColor *= particleAlpha;
		#endif
		#if _ALPHAMODULATE_ON
			particleColor.rgb = lerp(float3(1,1,1), particleColor.rgb, particleAlpha);
		#endif

		#if _CFXR_DISSOLVE
			// Dissolve
			half time = lerp(-_DissolveSmooth, 1+_DissolveSmooth, dissolveTime);
			particleAlpha *= smoothstep(dissolve - _DissolveSmooth, dissolve + _DissolveSmooth, time);
		#endif

		#if _ALPHATEST_ON
			clip(particleAlpha - _Cutoff);
		#endif

		#if !PASS_SHADOW_CASTER
			// Fog & Soft Particles
			applyFog(i, particleColor, particleAlpha);
			fragSoftParticlesFade(i, particleAlpha);
		#endif

			// Prevent alpha from exceeding 1
			particleAlpha = min(particleAlpha, 1.0);

		#if !PASS_SHADOW_CASTER
			return float4(particleColor, particleAlpha);
		#else

			//--------------------------------------------------------------------------------------------------------------------------------
			// Shadow Caster Pass

		#if _CFXR_ADDITIVE
			half alpha = max(particleColor.r, max(particleColor.g, particleColor.b)) * particleAlpha;
		#else
			half alpha = particleAlpha;
		#endif

		#if (_CFXR_DITHERED_SHADOWS_ON || _CFXR_DITHERED_SHADOWS_CUSTOMTEXTURE) && !defined(SHADER_API_GLES)
			alpha = min(alpha, _ShadowStrength);
			// Use dither mask for alpha blended shadows, based on pixel position xy
			// and alpha level. Our dither texture is 4x4x16.
			#if _CFXR_DITHERED_SHADOWS_CUSTOMTEXTURE
			half texSize = _DitherCustom_TexelSize.z;
			alpha = tex3D(_DitherCustom, float3(vpos.xy*(1 / texSize), alpha*(1 - (1 / (texSize*texSize))))).a;
			#else
			alpha = tex3D(_DitherMaskLOD, float3(vpos.xy*0.25, alpha*0.9375)).a;
			#endif
		#endif
			clip(alpha - 0.01);
			SHADOW_CASTER_FRAGMENT(i)
		#endif
		}