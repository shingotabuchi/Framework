Shader "Hidden/KawaseBlur"
{
    Properties { _MainTex("Texture",2D)="white" {} }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass   // 0 = down-sample
        {
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 frag(v2f_img i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDHLSL
        }
        Pass   // 1 = Kawase 5-tap blur
        {
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float2 _MainTex_TexelSize;
            float4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float4 col = tex2D(_MainTex, uv)                     * 4.0;
                col += tex2D(_MainTex, uv + _MainTex_TexelSize.xy);
                col += tex2D(_MainTex, uv - _MainTex_TexelSize.xy);
                col += tex2D(_MainTex, uv + float2(_MainTex_TexelSize.x,-_MainTex_TexelSize.y));
                col += tex2D(_MainTex, uv - float2(_MainTex_TexelSize.x,-_MainTex_TexelSize.y));
                return col / 8.0;
            }
            ENDHLSL
        }
    }
}
