//https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
//http://catlikecoding.com/unity/tutorials/rendering/part-9/
Shader "Unlit/Wireframe"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_WireColor("WireColor", Color) = (0,0,0, 1)
		_Width("width", Range(0, 10)) = 1

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 color: TEXTCOORD1;
				
				UNITY_FOG_COORDS(1)
			};

			float4 _Color;
			float4 _WireColor;
			//float _Blur;
			float _Width;

			float4 _MainTex_ST;
			v2f vert (appdata v)
			{
				v2f o;
				
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color.xyz;
				
				UNITY_TRANSFER_FOG(o,o.vertex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{

				/*
				float4 col = float4(1.,1.,1.,1.);
				if ( min( min( i.color.x, i.color.y ), i.color.z) < _Width ) {
					col.xyz = _WireColor.xyz;
				}
				else {
					col.xyz = _Color.xyz;
				}
				*/
				
				float d = ( abs( ddx( i.color ) ) + abs( ddy( i.color ) ) ) * _Width;
				float3 a3 = smoothstep( float3( 0.,0.,0. ), float3(d, d, d), i.color );
				float value = min( min( a3.x, a3.y ), a3.z );
				float4 col = (_WireColor * (1 - value) + value * _Color );
				
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
