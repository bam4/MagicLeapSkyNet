Shader "Hidden/Paint in 3D/Fill"
{
	Properties
	{
		_Texture("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_Opacity("Opacity", Float) = 1

		_SrcRGB("Src RGB", Int) = 1 // 1 = One
		_DstRGB("Dst RGB", Int) = 1 // 1 = One
		_SrcA("Src A", Int) = 1 // 1 = One
		_DstA("Dst A", Int) = 1 // 1 = One
		_Op("Op", Int) = 0 // 0 = Add
	}
	SubShader
	{
		Tags
		{
			"Queue"           = "Transparent"
			"RenderType"      = "Transparent"
			"IgnoreProjector" = "True"
			"Paint in 3D"     = "True"
		}
		Pass
		{
			Blend[_SrcRGB][_DstRGB],[_SrcA][_DstA]
			BlendOp[_Op]
			Cull Off
			Lighting Off
			ZWrite Off

			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag
				#include "BlendModes.cginc"
				// AlphaBlend, Additive, Swapped AlphaBlend, Shape Lerp, Multiply
				#pragma multi_compile __ P3D_A P3D_B P3D_C P3D_D

				sampler2D _Buffer;
				sampler2D _Texture;
				float4    _Color;
				float     _Opacity;

				struct a2v
				{
					float4 vertex    : POSITION;
					float2 texcoord0 : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex    : SV_POSITION;
					float2 texcoord0 : TEXCOORD0;
				};

				struct f2g
				{
					float4 color : COLOR;
				};

				void Vert(a2v i, out v2f o)
				{
					o.vertex    = float4(i.texcoord0.xy * 2.0f - 1.0f, 0.5f, 1.0f);
					o.texcoord0 = i.texcoord0;
#if UNITY_UV_STARTS_AT_TOP
					o.texcoord0.y = 1.0f - o.texcoord0.y;
#endif
				}

				void Frag(v2f i, out f2g o)
				{
					float4 color = tex2D(_Texture, i.texcoord0) * _Color;

					//o.color = Blend(color, _Opacity, _Buffer, i.texcoord0);
					o.color = BlendMinimum(color, _Opacity, _Buffer, i.texcoord0, 1.0f / 255.0f);
				}
			ENDCG
		} // Pass
	} // SubShader
} // Shader