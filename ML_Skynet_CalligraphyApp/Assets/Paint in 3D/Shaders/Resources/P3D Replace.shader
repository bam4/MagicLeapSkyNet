Shader "Hidden/Paint in 3D/Replace"
{
	Properties
	{
		_Texture("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
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
			Blend One Zero
			Cull Off
			Lighting Off
			ZWrite Off

			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag

				sampler2D _Texture;
				float4    _Color;

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
					o.color = tex2D(_Texture, i.texcoord0) * _Color;
				}
			ENDCG
		} // Pass
	} // SubShader
} // Shader