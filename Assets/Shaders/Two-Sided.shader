Shader "Custom/Two-Sided"
{
	Properties
	{
		_Color1("_Color1", Color) = (1, 1, 1, 1)
		_Color2("_Color2", Color) = (1, 1, 1, 1)
		_Alpha1 ("_Alpha1", Float)= 1.0
		_Alpha2 ("_Alpha2", Float)= 1.0
		_Shininess ("Shininess", Float) = 10 //Shininess
        _SpecColor ("Specular Color", Color) = (1, 1, 1, 1) //Specular highlights color
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" } 
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag 

			#include "UnityCG.cginc"

			float4 _Color1;
			float4 _Color2;
			float _Alpha1;
			float _Alpha2;
			uniform float4 _LightColor0;
			uniform float4 _SpecColor;
            uniform float _Shininess;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 objVertex: float4;
				float3 normal : NORMAL; 
				float3 vertexInWorldCoords : TEXCOORD1;
			};

			//float4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				o.objVertex = v.vertex;
				o.vertexInWorldCoords = mul(unity_ObjectToWorld, v.vertex);
				o.normal = v.normal; //Normal 
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{

				// fixed4 col = _Color;
				float3 Kd;
				if (i.objVertex.x < -0.01)
				{
					//_Color1.a = _Alpha1;
					Kd = _Color1;
				}
				else if(i.objVertex.x > 0.01)
				{
					//_Color2.a = _Alpha2;
					Kd = _Color2;
				}
				else
				{
					Kd = float4(0, 0, 0, 1);
				}
				float3 P = i.vertexInWorldCoords.xyz;
                float3 N = normalize(i.normal);
                float3 V = normalize(_WorldSpaceCameraPos - P);
                float3 L = normalize(_WorldSpaceLightPos0.xyz - P);
                float3 H = normalize(L + V);
                
                //float3 Kd = _Color.rgb; //Color of object
                float3 Ka = UNITY_LIGHTMODEL_AMBIENT.rgb; //Ambient light
                //float3 Ka = float3(0,0,0); //UNITY_LIGHTMODEL_AMBIENT.rgb; //Ambient light
                float3 Ks = _SpecColor.rgb; //Color of specular highlighting
                float3 Kl = _LightColor0.rgb; //Color of light
                
                
                //AMBIENT LIGHT 
                float3 ambient = Ka;
                
               
                //DIFFUSE LIGHT
                float diffuseVal = max(dot(N, L), 0);
                float3 diffuse = Kd * Kl * diffuseVal;
                
                
                //SPECULAR LIGHT
                float specularVal = pow(max(dot(N,H), 0), _Shininess);
                
                if (diffuseVal <= 0) {
                    specularVal = 0;
                }
                
                float3 specular = Ks * Kl * specularVal;
                
                //FINAL COLOR OF FRAGMENT
                return float4(ambient + diffuse + specular, 1.0);
               
			}
			ENDCG
		}
	}
    
    FallBack "Diffuse"
}

