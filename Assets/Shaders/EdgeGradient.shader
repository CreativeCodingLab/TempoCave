// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/EdgeGradient" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		[PerRendererData]_Color1("Endpoint1", Color) = (1,1,1,1)
		[PerRendererData]_Color2("Endpoint2", Color) = (1,1,1,1)
		[PerRendererData]_Alpha("Alpha", Float) = 1.0
		_DashAmount("Dash Amount", Range(0, 50)) = 0
		_WhiteBalance("White Balance", Range(0, 1)) = 0
		[PerRendererData]_isNegativeConnectivity("isNegativeConnectivity", Float) = 0
		[PerRendererData]_isEdgeBundling("isEdgeBundling", Float) = 0
		[PerRendererData]_edgeNumber("edgeNumber", Int) = 0
		//[PerRendererData]_length("length", Float) = 0
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		LOD 200
	    //ZWrite On
		//Cull Back
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert alpha:fade 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		float4 _Color1;
		float4 _Color2;
		float _Alpha;
		float _DashAmount;
		float _WhiteBalance;
		
		//float _length;

		struct Input {
			float2 uv_MainTex;
			float3 vertInObjSpace;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _isNegativeConnectivity;
		float _isEdgeBundling;
		int _edgeNumber;

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.vertInObjSpace = v.vertex.xyz;
		}
		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			float4 col;
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) ;
			//if (_isEdgeBundling == 0) 
			{
				c = c * lerp(float4(1, 1, 1, 1), lerp(_Color1, _Color2, IN.uv_MainTex.y), _WhiteBalance);
				//c = c * lerp(float4(1, 1, 1, 1), lerp(float4(0, 1, 0, 1), float4(1, 0, 0, 1), IN.uv_MainTex.y), _WhiteBalance);
				//if (_isNegativeConnectivity == 1)
				//{
					//if (IN.uv_MainTex.g > clamp(_SinTime.w, 0, 1) && IN.uv_MainTex.g - 0.08 < clamp(_SinTime.w, 0, 1))
					/*if (sin(5*IN.uv_MainTex.g-_Time.y) > 0.9)
						c = tex2D(_MainTex, IN.uv_MainTex) * float4(1, 0, 0, 1);
					else
						c = tex2D(_MainTex, IN.uv_MainTex) * c;*/

					//if (IN.vertInObjSpace.y > -0.1 && IN.vertInObjSpace.y < 0.1)
						//c = float4(1.0, 0.0, 0.0, 1.0);

				//}
				//else
				//{
					//if (IN.vertInObjSpace.y > -0.1 && IN.vertInObjSpace.y < 0.1)
						//c = float4(0.0, 1.0, 0.0, 1.0);
				//}
			}
			/*else
			{
				if (_edgeNumber == 15 || _edgeNumber == 16 || _edgeNumber == 17)
					c = c * lerp(_Color1, _Color2, IN.uv_MainTex.y); 
				else
					c = c * lerp(float4(1, 1, 1, 1), lerp(_Color1, _Color2, IN.uv_MainTex.y), _WhiteBalance);
			}*/
				
			
			
				/*if (!step(sin(10 * IN.vertInObjSpace.y* _SinTime.w), 0.9f))
					c = float4(1,0,0,1);*/

			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * lerp(_Color1, _Color2, IN.uv_MainTex.y) * col;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = _Alpha;
			if (step(sin(_DashAmount * IN.vertInObjSpace.y), 0.5f) && _DashAmount > 0) discard;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
