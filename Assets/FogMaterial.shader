Shader "Custom/FogMaterial" {
    Properties {
        _Color ("Couleur du brouillard", Color) = (0.5, 0.5, 0.5, 0.5)
        _Opacity ("Opacité", Range(0, 1)) = 0.7
        
        _EdgeSoftness ("Douceur des bords", Range(0, 1)) = 0.5
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        fixed4 _Color;
        float _Opacity;
        float _EdgeSoftness;

        struct Input {
            float2 uv_MainTex;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 c = _Color;
            
            float edgeFactor = 1.0 - abs(dot(IN.viewDir, o.Normal));
            c.a *= _Opacity * pow(edgeFactor, 1.0 / _EdgeSoftness);

            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
