Shader "Unlit/ToonFaceShader"
{
    Properties
    {
        _AmbientColor ("Ambient Color", Color) = (0.667, 0.667, 0.667, 1)
        _DiffuseColor ("Diffuse Color", Color) = (0.906, 0.906, 0.906, 1)
        _ShadowColor ("Shadow Color", Color) = (0.737, 0.737, 0.737, 1)

        _BaseTexFac ("Base Tex Fac", Range(0, 1)) = 1
        _BaseTex ("Base Tex", 2D) = "white" {}
        _ToonTexFac ("Toon Tex Fac", Range(0, 1)) = 1
        _ToonTex ("Toon Tex", 2D) = "white" {}
        _SphereTexFac ("Sphere Tex Fac", Range(0, 1)) = 1
        _SphereTex ("Sphere Tex", 2D) = "white" {}
        _SphereMulAdd ("Sphere Mul/Add", Range(0, 1)) = 0

        _DoubleSided ("Double Sided", Range(0, 1)) = 0
        _Alpha("Alpha", Range(0, 1)) = 1

        _ShadowTex ("Shadow Tex", 2D) = "black" {}

        _SDF ("SDF", 2D) = "black" {}
        _ForwardVector ("Forward Vector", Vector) = (0, 0, 1, 0)
        _RightVector ("Right Vector", Vector) = (1, 0, 0, 0)

        _RampTex ("Ramp Tex", 2D) = "white" {}
        _RampRow ("Ramp Row ", Range(1, 5)) = 2

        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 0)
        _OutlineOffset ("Outline Offset", float) = 0.000015
    }
    SubShader
    {
        LOD 100

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            // -------------------------------------
            // Universal Pipeline keywords

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        
        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DrawObject"
            Tags {
                "RenderPipeline" = "UniversalPipeline"
                "RenderType" = "Opaque"
                "LightMode" = "UniversalForward"
            }
            Cull Off

            HLSLPROGRAM
            #pragma multi_compile _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _SHADOWS_SDFT

            #pragma vertex vert
            #pragma fragment frag
            //Make fog work
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                half4 color : COLOR0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;

                float3 positionWS : TEXCOORD1;
                float3 positionVS : TEXCOORD2;
                float4 positionCS : SV_POSITION;
                float4 positionNDC : TEXCOORD3;

                float3 normalWS : TEXCOORD4;
                float3 tangentWS : TEXCOORD5;
                float3 bitangentWS : TEXCOORD6;

                float fogCoord : TEXCOORD7;
                float4 shadowCoord : TEXCOORD8;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _AmbientColor;
            float4 _DiffuseColor;
            float4 _ShadowColor;

            half _BaseTexFac;
            sampler2D _BaseTex;
            sampler2D _SkinTex;
            float4 _BaseTex_ST;
            half _ToonTexFac;
            sampler2D _ToonTex;
            half _SphereTexFac;
            sampler2D _SphereTex;
            half _SphereMulAdd;

            half _DoubleSided;
            half _Alpha;

            sampler2D _SDF;
            float3 _ForwardVector;
            float3 _RightVector;

            sampler2D _ShadowTex;

            sampler2D _RampTex;
            float _RampRow;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseTex);
                o.positionWS = vertexInput.positionWS;
                o.positionVS = vertexInput.positionVS;
                o.positionCS = vertexInput.positionCS;
                o.positionNDC = vertexInput.positionNDC;

                VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(v.normal, v.tangent);
                o.tangentWS = vertexNormalInput.tangentWS;
                o.bitangentWS = vertexNormalInput.bitangentWS;
                o.normalWS = vertexNormalInput.normalWS;

                o.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                o.shadowCoord = TransformWorldToShadowCoord(vertexInput.positionWS);

                return o;
            }

            float4 frag(v2f i, bool isFacing : SV_IsFrontFace) : SV_Target
            {
                Light light = GetMainLight(i.shadowCoord);

                //N 世界空间法线向量    V 摄像机向量      L 光线向量      H 半角向量
                float3 N = normalize(i.normalWS);
                float3 V = normalize(mul((float3x3)UNITY_MATRIX_I_V, i.positionVS * (-1)));
                float3 L = normalize(light.direction);

                float NoV = dot(N, V);

                float3 normalVS = normalize(mul((float3x3)UNITY_MATRIX_V, N));
                float2 matcapUV = normalVS.xy * 0.5 + 0.5;

                float4 baseTex = tex2D(_BaseTex, i.uv);
                float4 toonTex = tex2D(_ToonTex, matcapUV);
                float4 sphereTex = tex2D(_SphereTex, matcapUV);

                float3 baseColor = _AmbientColor.rgb;
                baseColor = saturate(lerp(baseColor, baseColor + _DiffuseColor.rgb, 0.6));
                baseColor = lerp(baseColor, baseColor * baseTex.rgb, _BaseTexFac);
                baseColor = lerp(baseColor, baseColor * toonTex.rgb, _ToonTexFac);
                baseColor = lerp(
                    lerp(baseColor, baseColor * sphereTex.rgb, _SphereTexFac), 
                    lerp(baseColor, baseColor + sphereTex.rgb, _SphereTexFac),
                     _SphereMulAdd);

                float rampV = _RampRow / 10 - 0.05;
                float rampClampMin = 0.003;
                float2 rampDayUV = float2(rampClampMin, 1 - rampV);
                float2 rampNightUV = float2(rampClampMin, 1 - (rampV + 0.5));
               
                float isDay = (L.y + 1) / 2;
                float rampColor = lerp(tex2D(_RampTex, rampNightUV).rgb, tex2D(_RampTex, rampDayUV).rgb, isDay);

                float3 forwardVec = _ForwardVector;
                float3 rightVec = _RightVector;

                float3 upVector = cross(forwardVec, rightVec);
                //float3 LpU = length(L) * (dot(L, upVector) / (length(L) * length(upVector))) * (upVector / length(upVector));
                float3 LpU = dot(L, upVector) / pow(length(upVector), 2) * upVector;
                float3 LpHeadHorizon = L - LpU;

                float pi = 3.141592654;
                float value = acos(dot(normalize(LpHeadHorizon), normalize(rightVec))) / pi;
                //0 ~ 0.5 expose right, 0.5 ~ 1 expose left
                float exposeRight = step(value, 0.5);

                //right: 1 ~ 0
                float valueR = pow(1 - value * 2, 4);
                //left: 0 ~ 1
                float valueL = pow(value * 2 - 1, 4);
                float mixValue = lerp(valueL, valueR, exposeRight);

                float sdfRembrandtLeft = tex2D(_SDF, float2(1 - i.uv.x, i.uv.y)).r;
                float sdfRembrandtRight = tex2D(_SDF, i.uv).r;
                float mixSdf = lerp(sdfRembrandtRight, sdfRembrandtLeft, exposeRight);

                float sdf = step(mixValue, mixSdf);
                sdf = lerp(0, sdf, step(0, dot(normalize(LpHeadHorizon), normalize(forwardVec))));

                float4 shadowTex = tex2D(_ShadowTex, i.uv);
                sdf *= shadowTex.g;
                sdf = lerp(sdf, 1, shadowTex.a);

                float3 shadowColor = baseColor * rampColor * _ShadowColor.rgb;

                float3 diffuse = lerp(shadowColor, baseColor, sdf);
                //diffuse = lerp(shadowColor, diffuse, light.shadowAttenuation);  //投影

                float3 albedo = diffuse;

                float rimOffset = 20;
                float rimThreshold = 0.03;
                float rimStrength = 0.6;
                float rimMax = 0.5;

                float2 screenUV = i.positionNDC.xy / i.positionNDC.w;
                float rawDepth = SampleSceneDepth(screenUV);
                float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float2 screenOffset = float2(lerp(-1, 1, step(0, normalVS.x)) * rimOffset / _ScreenParams.x / max(1, pow(linearDepth, 2)), 0);
                float offsetDepth = SampleSceneDepth(screenUV + screenOffset);
                float offsetLinearDepth = LinearEyeDepth(offsetDepth, _ZBufferParams);

                float rim = saturate(offsetLinearDepth - linearDepth);
                rim = step(rimThreshold, rim) * clamp(rim * rimStrength, 0, rimMax);

                float fresnelPower = 2;
                float fresnelClamp = 0.8;
                float fresnel = 1 - saturate(NoV);
                fresnel = pow(fresnel, fresnelPower);
                fresnel = fresnel * fresnelClamp + (1 - fresnelClamp);

                albedo = 1 - (1 - rim * fresnel) * (1 - albedo);

                float alpha = _Alpha * baseTex.a * toonTex.a * sphereTex.a;
                alpha = saturate(min(max(isFacing, _DoubleSided), alpha));

                float4 col = float4(albedo, alpha);
                clip(col.a - 0.5);

                col.rgb = MixFog(col.rgb, i.fogCoord);

                return col;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DrawOutline"
            Tags{
                "RenderPipeline" = "UniversalPipeline"
                "RenderType" = "Opaque"
            }
            Cull Front

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            //Make fog work
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                half4 color : COLOR0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float fogCoord : TEXCOORD7;
            };

            CBUFFER_START(UnityPerMaterial)
            sampler2D _BaseTex;
            float4 _BaseTex_ST;

            float4 _OutlineColor;
            float _OutlineOffset;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                //VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz + v.normal.xyz * _OutlineOffset);
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz + v.tangent.xyz * _OutlineOffset);
                o.uv = TRANSFORM_TEX(v.uv, _BaseTex);
                o.positionCS = vertexInput.positionCS;
                o.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                return o;
            }

            float4 frag(v2f i, bool isFacing : SV_IsFrontFace) : SV_Target
            {
                float4 col = _OutlineColor;
                col.rgb = MixFog(col.rgb, i.fogCoord);
                return col;
            }
            ENDHLSL
        }
    }
}
