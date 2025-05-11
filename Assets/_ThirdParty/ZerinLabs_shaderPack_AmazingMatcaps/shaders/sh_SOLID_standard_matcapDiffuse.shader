Shader "zerinlabs/sh_SOLID_standard_matcapDiffuse"
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
		_mc_diff("Diffuse Vs. Matcap (mix)", Range(0, 1)) = 1

		[Toggle(MATCAP_MULTI)] _mc_multi("Multiply matcap", Float) = 0	//if ticked multiply matcap over diffuse
		[Toggle(MATCAP_ADD)] _mc_add("Add matcap", Float) = 0			//if ticked add matcap over diffuse
		
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
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
		}

		LOD 200

		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.5 //<--- For very low end devices you could change this by "#pragma target 3.0" and remove all the vertex colour operations
		#pragma shader_feature MATCAP_MULTI
		#pragma shader_feature MATCAP_ADD
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

		uniform float _mc_diff;
		uniform float _mc_multi;
		uniform float _mc_add;
		uniform float _overBright;

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
			
			float3 objSpaceCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1)).xyz * 1.0;
			
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

			float4 MC = tex2D(_MC, matCap_uv) * _Tint;

			#if MATCAP_MULTI //if ticked multiply matcap over diffuse
				DF = DF * lerp(fixed4(1.0, 1.0, 1.0, 1.0), MC, _mc_diff * MK.a);
			#elif MATCAP_ADD //if ticked add matcap over diffuse
				DF = DF + (MC * _mc_diff * MK.a);
			#else
				DF = lerp(DF, MC, _mc_diff * MK.a);
			#endif

			//vertex colour properties ..................................................
			#if VERTEXCOLOR
				fixed4 VC = IN.vc_col;

				#if VERTEXCOLORTINT
					float VC_bw = dot(IN.vc_col, float3(0.3, 0.59, 0.11)); //properly weighted greyscale 
					VC = lerp(_VC_tint_low, _VC_tint_hi, VC_bw);
				#endif

				DF = DF * VC * _vcIntensity;
			#endif

			DF = DF * _overBright;// *lerp(fixed4(1.0, 1.0, 1.0, 1.0), , MK.a);

			o.Albedo = DF.rgb;
			o.Alpha = DF_alpha;
			o.Normal = NM;

			//R/G/B = metallic/smoothness/occlusion
			o.Metallic = MK.r * _MetallicFactor;
			o.Smoothness = MK.g * _GlossMapScale;
			o.Occlusion = lerp(1.0, MK.b, _AOamount);
		}

		ENDCG

	}//SubShader

	FallBack "Diffuse"
}