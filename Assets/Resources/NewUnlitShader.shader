Shader "Unlit/NewUnlitShader"{
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Strength ("Strength", Float) = 2
    }
    SubShader {
        Tags {"RenderType"="Overlay"}
        ZWrite off Cull off // NOTE: 'Cull off' is important as the halo meshes flip handedness each time... BUG: #1220
        Blend OneMinusDstColor One
        ColorMask RGB
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            fixed4 _Color;
            fixed _Strength;
            fixed4 fog = {0,0,0,0};
            sampler2D _HaloFalloff;
            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            float4 _HaloFalloff_ST;
            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_HaloFalloff);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed2 p;
                p.x=i.texcoord.x+0.5;
                p.y=i.texcoord.y+0.5;
                fixed a = tex2D(_HaloFalloff, p).a;
                a=a*a/_Strength;
                fixed4 col = fixed4 (_Color.rgb * a, a);
                return col;
            }
            ENDCG
        }
    }
}
