Shader "Custom/Node"
{
    Properties
    {
		[PerRendererData]_Color1("_Color1", Color) = (1, 1, 1, 1)
		[PerRendererData]_Color2("_Color2", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		//_GlyphSize("GlyphSize", Float) = 1.0
	    [PerRendererData]_Alpha("Alpha", Float) = 1.0
	    [PerRendererData]_Alpha1("Alpha1", Float) = 1.0
	    [PerRendererData]_Alpha2("Alpha2", Float) = 1.0
    }
    SubShader
    {
        //Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		float4 _Color1;
		float4 _Color2;
		
		float _Alpha1;
		float _Alpha2;
		//float _GlyphSize;

        struct Input
        {
            float2 uv_MainTex;
			float3 vertexInWorldCoords;
			//float3 worldNormal;
        };

		void vert (inout appdata_full v, out Input o) 
		{
		    UNITY_INITIALIZE_OUTPUT(Input,o);
			//v.vertex.xyz += v.normal * _GlyphSize;
			o.vertexInWorldCoords = v.vertex.xyz;
		}

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float4 col;
			float _Alpha;
			if (IN.vertexInWorldCoords.x < 0)
			{
				col = _Color1;
				_Alpha = _Alpha1;
			}
			else
			{
				col = _Color2;
				_Alpha = _Alpha2;
			}
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * col;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = _Alpha;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
