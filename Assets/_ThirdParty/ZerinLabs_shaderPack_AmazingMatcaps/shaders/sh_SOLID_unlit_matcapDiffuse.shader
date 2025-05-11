Shader "zerinlabs/sh_SOLID_unlit_matcapDiffuse" 
{
	Properties
	{
		[Header(GENERAL)]
		_overBright("Overbright", Float) = 1
		_Tint("Tint", color) = (1.0, 1.0, 1.0, 1.0)

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

		Pass
		{
			CGPROGRAM

			//#pragma surface surf Standard alphatest:_Cutoff vertex:vert addshadow
			#pragma vertex vert 
			#pragma fragment frag
			#pragma shader_feature MATCAP_MULTI
			#pragma shader_feature MATCAP_ADD 
			#pragma	shader_feature VERTEXCOLOR
			#pragma shader_feature VERTEXCOLORTINT

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			uniform sampler2D _MC;
			uniform sampler2D _DF;
			uniform sampler2D _NM;
			uniform sampler2D _MK;

			uniform float _BumpScale;
			uniform float _overBright;
			uniform float _AOamount;
			uniform float _mc_diff;

			uniform fixed4 _Tint;

			uniform float _vcIntensity;
			uniform fixed4 _VC_tint_hi;
			uniform fixed4 _VC_tint_low;

			struct vertexInput
			{
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				//float4 texcoord1 : TEXCOORD1;
				fixed4 color : COLOR;
				//half4 texcoord2 : TEXCOORD2;
				//half4 texcoord3 : TEXCOORD3;
				//half4 texcoord4 : TEXCOORD4;
				//half4 texcoord5 : TEXCOORD5;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float2 df_uv : TEXCOORD0;

				float3 matcapCoord1 : TEXCOORD1;
				float3 matcapCoord2 : TEXCOORD2;

				fixed4 vc_col : COLOR;

				LIGHTING_COORDS(3, 4)
			};

			float4 _DF_ST;

			vertexOutput vert(vertexInput v)
			{
				vertexOutput OUT;
				OUT.df_uv = TRANSFORM_TEX(v.texcoord0, _DF);

				float4 vPos = UnityObjectToClipPos(v.vertex);
				OUT.pos = vPos; 

				v.normal = normalize(v.normal);
				v.tangent = normalize(v.tangent);

				TANGENT_SPACE_ROTATION; // This macro defines the object-to-tangent matrix called rotation.
				OUT.matcapCoord1 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[0].xyz));
				OUT.matcapCoord2 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[1].xyz));

				OUT.vc_col = v.color;

				TRANSFER_VERTEX_TO_FRAGMENT(OUT);

				return OUT;
			}

			half4 frag(vertexOutput IN) : COLOR
			{
				fixed4 MK = tex2D(_MK, IN.df_uv);
				fixed4 DF = tex2D(_DF, IN.df_uv);

				fixed3 NM = UnpackNormal(tex2D(_NM, IN.df_uv));
				NM = lerp(fixed3(0, 0, 1), NM, _BumpScale);

				half2 matCap_uv = half2(dot(IN.matcapCoord1, NM), dot(IN.matcapCoord2, NM)) * 0.5 + 0.5;
				float4 MC = tex2D(_MC, matCap_uv) * _Tint;;

				//blending colour properties ..................................................
				#if MATCAP_MULTI //if ticked multiply matcap over diffuse
					DF = DF * lerp(fixed4(1.0,1.0,1.0,1.0), MC, _mc_diff) * MK.a;
				#elif MATCAP_ADD //if ticked add matcap over diffuse
					DF = (DF + (MC * _mc_diff)) * MK.a;
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

				float AO = lerp(1.0, MK.b, _AOamount);

				DF = DF * _overBright * AO;

				fixed4 Complete = DF;

				return Complete;
			}

			ENDCG

		} //end pass

	}//SubShader

	Fallback "Diffuse"
}