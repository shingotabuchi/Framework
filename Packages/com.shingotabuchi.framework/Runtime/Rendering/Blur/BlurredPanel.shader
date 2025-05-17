Shader "UI/BlurredPanel"
{
    Properties
    {
        _Color   ("Tint",  Color) = (1,1,1,1)
        _MainTex ("Fallback Sprite", 2D) = "white" {}   // kept for SpriteInspector, not sampled
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Pass
        {
            // Standard UI blend / depth settings
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _BlurTex;        // ↙ set every frame by your blur RenderFeature
            float4    _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 tex    : TEXCOORD0;   // still pass through UVs so slices/masks line up
                float4 color  : COLOR;       // per-vertex tint from CanvasRenderer
            };

            struct v2f
            {
                float4 pos  : SV_POSITION;
                half2  uv   : TEXCOORD0;
                fixed4 col  : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.tex;              // same UV we got from the Image/Sprite
                o.col = v.color * _Color;   // multiplies Canvas tint × Material tint
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the pre-blurred screen grab
                fixed4 c = tex2D(_BlurTex, i.uv);

                // Apply UI vertex/material tint & alpha
                return c * i.col;
            }
            ENDHLSL
        }
    }

    // Fallback keeps inspector thumbnails and ETC variants happy
    Fallback "UI/Default"
}
