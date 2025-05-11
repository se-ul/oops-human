// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "zerinlabs/sh_BLEND_standard_matcapDiffuse"
{
	Properties
	{
		[Header(GENERAL)]
		_overBright("Overbright", Float) = 1
		_Tint("Tint", color) = (1.0, 1.0, 1.0, 1.0)
		
		[Space(20)]
		_MetallicFactor("Metallic Factor",Range(0, 1)) = 0
		_GlossMapScale("Smoothness", Range(0, 1)) = 0

		[Header(VERTEX COLOR)]
		[Toggle(VERTEXCOLOR)] _vc_use("Use vertex color", Float) = 0			//if ticked multiply VC over all
		_vcIntensity(" - VC intensity", Float) = 1

		[Space(20)]
		[Toggle(VERTEXCOLORTINT)] _vc_usetint("Override vertex color", Float) = 0			//if ticked multiply VC over all
		_VC_tint_hi(" - VC tint (hi)", color) = (1.0, 1.0, 1.0, 1.0)
		_VC_tint_low(" - VC tint (low)", color) = (0.0, 0.0, 0.0, 1.0)

		[Header(MATCAP)]
		[NoScaleOffset]
		_MC("MC (MatcapMap)", 2D) = "white" {}
		_mc_df_mix("Diffuse Vs. Matcap (mix)", Range(0, 1)) = 1
		
		[Toggle(DF_ALPHA)] _dfAlpha("Use Diffuse greyscale as alpha", Float) = 0	//if ticked transforms diffusse as gresycale and use it as alpha
		[Toggle(MC_ALPHA)] _mcAlpha("Use Matcap greyscale as alpha", Float) = 0	//if ticked transforms matcap as gresycale and use it as alpha
		
		[Space(20)]
		_alpha("Global alpha multiplier", Range(0,1)) = 1

		[Header(OTHER TEXTURES)]
		_DF("DF (DiffuseMap)", 2D) = "white" {}
		
		[NoScaleOffset]
		_NM("NM (NormalMap)", 2D) = "white" {}
		_BumpScale("Bump amount", Float) = 1
		[Space(20)]
		[NoScaleOffset]
		_MK("Mask (R/G/B/A = metallic/smoothness/occlusion/matcap)", 2D) = "white" {}
		_AOamount("AO contribution", Range(0, 1)) = 1
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		LOD 200

		//Cull Off		//Cull = Back | Front | Off (Off = two sided -  enable if add/multiply)
		//Lighting Off	// On / Off
		ZWrite On		//On / Off

		//Blending mode----------------------------------------------
		Blend SrcAlpha OneMinusSrcAlpha			// Traditional transparency
		//Blend One OneMinusSrcAlpha				// Premultiplied transparency (alpha blend)
		//Blend One One								// Additive
		//Blend OneMinusDstColor One				// Soft Additive
		//Blend DstColor Zero						// Multiplicative
		//Blend DstColor SrcColor					// 2x Multiplicative

		CGPROGRAM

			#pragma surface surf Standard fullforwardshadows vertex:vert alpha:fade
			#pragma target 3.5 //<--- For very low end devices you could change this by "#pragma target 3.0" and remove all the vertex colour operations
			#pragma shader_feature DF_ALPHA
			#pragma shader_feature MC_ALPHA 
			#pragma	shader_feature VERTEXCOLOR
			#pragma shader_feature VERTEXCOLORTINT

			#include "UnityCG.cginc"

			uniform float _MetallicFactor;
			uniform float _GlossMapScale;

			uniform sampler2D _DF;
			uniform sampler2D _MC;
			uniform sampler2D _NM;
			uniform sampler2D _MK;
			uniform float _BumpScale;
			uniform float _AOamount;

			uniform float _mc_df_mix;
			uniform float _mc_multi;
			uniform float _mc_add;
			uniform float _overBright;
			uniform float _alpha;

			uniform fixed4 _Tint;

			uniform float _vcIntensity;
			uniform fixed4 _VC_tint_hi;
			uniform fixed4 _VC_tint_low;

			struct Input
			{
				float2 uv_DF;

				float3 matcapCoord1;
				float3 matcapCoord2;

				fixed4 vc_col : COLOR;
			};

			/*
			struct appdata_full
			{
				//float4 vertex : POSITION;
				//float4 tangent : TANGENT;
				//float3 normal : NORMAL;
				//fixed4 color : COLOR;
				//float4 texcoord : TEXCOORD0;
				//float4 texcoord1 : TEXCOORD1;
				//half4 texcoord2 : TEXCOORD2;
				//half4 texcoord3 : TEXCOORD3;
				//half4 texcoord4 : TEXCOORD4;
				//half4 texcoord5 : TEXCOORD5;
			};
			*/

			void vert(inout appdata_full v, out Input o)
			{
				UNITY_INITIALIZE_OUTPUT(Input, o);

				float4 vPos = UnityObjectToClipPos(v.vertex);

				v.normal = normalize(v.normal);
				v.tangent = normalize(v.tangent);

				TANGENT_SPACE_ROTATION; // This macro defines the object-to-tangent matrix called rotation.
				o.matcapCoord1 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[0].xyz));
				o.matcapCoord2 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[1].xyz));

				o.vc_col = v.color;
			}

			/*
			struct SurfaceOutputStandard
			{
				fixed3 Albedo;      // base (diffuse or specular) color
				fixed3 Normal;      // tangent space normal, if written
				half3 Emission;
				half Metallic;      // 0=non-metal, 1=metal
				half Smoothness;    // 0=rough, 1=smooth
				half Occlusion;     // occlusion (default 1)
				fixed Alpha;        // alpha for transparencies
			};
			*/

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				fixed4 MK = tex2D(_MK, IN.uv_DF);

				fixed4 DF = tex2D(_DF, IN.uv_DF);
				float DF_alpha = DF.a;

				fixed3 NM = UnpackNormal(tex2D(_NM, IN.uv_DF));
				NM = lerp(fixed3(0, 0, 1), NM, _BumpScale);

				half2 matCap_uv = half2(dot(IN.matcapCoord1, NM), dot(IN.matcapCoord2, NM)) * 0.5 + 0.5;
				fixed4 MC = tex2D(_MC, matCap_uv) * _Tint;
				float MCA = MC.a;
				
				//blending colour properties ..................................................
				#if MC_ALPHA //if ticked transform the matcap to grayscale and use it as alpha
					MC.a =  dot(MC.rgb, float3(0.3, 0.59, 0.11)); //properly weighted greyscale values
				#else
					MC.a = MCA;
				#endif

				#if DF_ALPHA //if ticked transform the diffuse to grayscale and use it as alpha
					DF.a = dot(DF.rgb, float3(0.3, 0.59, 0.11)); //properly weighted greyscale 
				#else
					DF.a = DF_alpha;
				#endif
				
				DF.rgb = lerp(DF.rgb, MC.rgb, _mc_df_mix * MK.a);

				//vertex colour properties ..................................................
				#if VERTEXCOLOR
					fixed4 VC = IN.vc_col;

					#if VERTEXCOLORTINT
						float VC_bw = dot(IN.vc_col, float3(0.3, 0.59, 0.11)); //properly weighted greyscale 
						VC = lerp(_VC_tint_low, _VC_tint_hi, VC_bw);
					#endif

					DF = DF * VC * _vcIntensity;
				#endif

				o.Albedo = DF.rgb * _overBright;
				o.Alpha = MC.a * DF.a * _alpha;

				o.Normal = NM;

				//R/G/B = metallic/smoothness/occlusion
				o.Metallic = MK.r * _MetallicFactor;
				o.Smoothness = MK.g * _GlossMapScale;
				o.Occlusion = lerp(1.0, MK.b, _AOamount);
			}
			
		ENDCG
	}

	FallBack "Diffuse"
}