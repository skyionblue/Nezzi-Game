Shader "Custom/OutlinePass"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor    ("Outline Color",    Color)                = (0.05, 0.05, 0.05, 1)
        _OutlineThickness("Outline Thickness",Float)                = 1.5
        _DepthThreshold  ("Depth Threshold",  Range(0.0001, 0.05)) = 0.004
        _NormalThreshold ("Normal Threshold", Range(0.0, 1.0))      = 0.3
        _OutlineStrength ("Outline Strength", Range(0.0, 1.0))      = 0.9
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Cull Off  ZWrite Off  ZTest Always

        Pass
        {
            Name "OutlinePass"

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag
            #pragma target 3.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            TEXTURE2D_X(_CameraNormalsTexture);
            SAMPLER(sampler_CameraNormalsTexture);

            float4  _OutlineColor;
            float   _OutlineThickness;
            float   _DepthThreshold;
            float   _NormalThreshold;
            float   _OutlineStrength;

            // Linear depth 0-1
            float SampleDepth(float2 uv)
            {
                float raw = SAMPLE_TEXTURE2D_X(_CameraDepthTexture,
                                               sampler_CameraDepthTexture, uv).r;
                return Linear01Depth(raw, _ZBufferParams);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float2 ts = _BlitTexture_TexelSize.xy * _OutlineThickness;

                // ── Depth Sobel ──────────────────────────────────────────────
                float d00 = SampleDepth(uv + float2(-ts.x,  ts.y));
                float d10 = SampleDepth(uv + float2(    0,  ts.y));
                float d20 = SampleDepth(uv + float2( ts.x,  ts.y));
                float d01 = SampleDepth(uv + float2(-ts.x,      0));
                float d21 = SampleDepth(uv + float2( ts.x,      0));
                float d02 = SampleDepth(uv + float2(-ts.x, -ts.y));
                float d12 = SampleDepth(uv + float2(    0, -ts.y));
                float d22 = SampleDepth(uv + float2( ts.x, -ts.y));

                float gx = -d00 - 2*d01 - d02 + d20 + 2*d21 + d22;
                float gy = -d00 - 2*d10 - d20 + d02 + 2*d12 + d22;
                float depthEdge = sqrt(gx*gx + gy*gy);
                float outline = step(_DepthThreshold, depthEdge);

                // ── Source colour ────────────────────────────────────────────
                half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                // Apply outline
                return lerp(col, _OutlineColor, outline * _OutlineStrength);
            }
            ENDHLSL
        }
    }
}
