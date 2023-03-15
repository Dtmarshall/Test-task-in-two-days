Shader "Toony/Metal"
{
	Properties
	{
		//TOONY COLORS
		_Color ("Color", Color) = (0.5,0.5,0.5,1.0)
		_SColor ("Shadow Color", Color) = (0.3,0.3,0.3,1.0)
		
		//DIFFUSE
		_MainTex ("Main Texture (RGB)", 2D) = "white" {}
		
		//TOONY COLORS RAMP
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
		
		[Header(Specular)]
		_SpecularIntensity ("Specular Intensity", Range(0, 64)) = 16
		_SpecularColor ("Specular Color", Color) = (1,1,1,0.6666)
		_SpecularSmooth ("Specular Smooth", Range(0, 0.5)) = 0

		[Header(Highlight)]
		_HighlightColor ("Highlight Color", Color) = (1,1,1,1)
		_HighlightOffset ("Highlight Offset", Range(0, 1)) = 0
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		
		#pragma surface surf ToonyColorsCustom interpolateview halfasview
		//#pragma target 2.0
		//#pragma glsl
		
		
		//================================================================
		// VARIABLES
		
		fixed4 _Color;
		sampler2D _MainTex;
		
		
		struct Input
		{
			half2 uv_MainTex;
		};
		
		//================================================================
		// CUSTOM LIGHTING
		
		//Lighting-related variables
		fixed4 _SColor;
		sampler2D _Ramp;
		
		//Custom SurfaceOutput
		struct SurfaceOutputCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			half Specular;
			fixed Alpha;
		};

		float _SpecularIntensity;
		float4 _SpecularColor;
		float _SpecularSmooth;

		float4 _HighlightColor;
		float _HighlightOffset;
		
		inline half4 LightingToonyColorsCustom (SurfaceOutputCustom s, half3 lightDir, half3 viewDir, half atten)
		{
			s.Normal = normalize(s.Normal);
			fixed ndl = max(0, dot(s.Normal, lightDir) * 0.5 + 0.5);
			
			fixed3 ramp = tex2D(_Ramp, fixed2(ndl,ndl));
			#if !(POINT) && !(SPOT)
				ramp *= atten;
			#endif

			float3 halfVector = normalize(lightDir + viewDir);
			float NdotH = saturate(dot(s.Normal, halfVector));
			float specularIntensity = smoothstep(_SpecularSmooth, 1 - _SpecularSmooth, pow(NdotH * ramp, _SpecularIntensity));
			float3 specular = specularIntensity * _SpecularColor.rgb * _SpecularColor.a;
			//Reflection

			float3 highlight = step(_HighlightOffset, dot(lightDir, s.Normal)) * _HighlightColor.rgb * _HighlightColor.a;
			//Highlight

			_SColor = lerp(fixed4(1, 1, 1, 1), _SColor, _SColor.a);
			ramp = lerp(_SColor.rgb, fixed3(1, 1, 1), ramp);

			fixed4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * ramp + specular + highlight;
			#if (POINT || SPOT)
				c.rgb *= atten;
			#endif

			c.a = s.Alpha;

			return c;
		}
		
		
		//================================================================
		// SURFACE FUNCTION
		
		void surf (Input IN, inout SurfaceOutputCustom o)
		{
			fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
			
			o.Albedo = mainTex.rgb * _Color.rgb;
			o.Alpha = mainTex.a * _Color.a;
		}
		
		ENDCG
	}
	
	Fallback "Diffuse"
}
