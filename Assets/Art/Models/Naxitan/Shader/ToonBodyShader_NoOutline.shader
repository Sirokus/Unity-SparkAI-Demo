Shader "Unlit/ToonBodyShader_NoOutline"
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

        _MetalTex ("Metal Tex", 2D) = "black" {}

        _SpecExpon ("Spec Exponent", Range(1, 128)) = 50
        _KsNonMetallic ("Ks Non-Metallic", Range(0, 3)) = 1
        _KsMetallic ("Ks Metallic", Range(0, 3)) = 1

        _NormalMap ("Normal Map", 2D) = "bump" {}
        _ILM ("ILM", 2D) = "black" {}

        _RampTex ("Ramp Tex", 2D) = "white" {}

        _RampMapRow0 ("Ramp Map Row 0", Range(1, 5)) = 1
        _RampMapRow1 ("Ramp Map Row 1", Range(1, 5)) = 2
        _RampMapRow2 ("Ramp Map Row 2", Range(1, 5)) = 3
        _RampMapRow3 ("Ramp Map Row 3", Range(1, 5)) = 4
        _RampMapRow4 ("Ramp Map Row 4", Range(1, 5)) = 5

        _OutlineOffset ("Outline Offset", float) = 0.000015

        _OutlineMapColor0 ("Outline Map Color 0", Color) = (0, 0, 0, 0)
        _OutlineMapColor1 ("Outline Map Color 1", Color) = (0, 0, 0, 0)
        _OutlineMapColor2 ("Outline Map Color 2", Color) = (0, 0, 0, 0)
        _OutlineMapColor3 ("Outline Map Color 3", Color) = (0, 0, 0, 0)
        _OutlineMapColor4 ("Outline Map Color 4", Color) = (0, 0, 0, 0)
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

            sampler2D _MetalTex;

            float _SpecExpon;
            float _KsNonMetallic;
            float _KsMetallic;

            sampler2D _NormalMap;
            sampler2D _ILM;

            sampler2D _RampTex;

            float _RampMapRow0;
            float _RampMapRow1;
            float _RampMapRow2;
            float _RampMapRow3;
            float _RampMapRow4;
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

                //NPR
                float4 normalMap = tex2D(_NormalMap, i.uv);
                float3 normalTS = float3(normalMap.ag * 2 - 1, 0);      //映射到 -1 ~ 1
                normalTS.z = sqrt(1 - dot(normalTS.xy, normalTS.xy));   //切线x，y，z分量的平方和为1，因此1 - xy分量平方和可得到z的平方和，再开个根号得到z分量

                //N 世界空间法线向量    V 摄像机向量      L 光线向量      H 半角向量
                float3 N = normalize(mul(normalTS, float3x3(i.tangentWS, i.bitangentWS, i.normalWS)));
                float3 V = normalize(mul((float3x3)UNITY_MATRIX_I_V, i.positionVS * (-1)));
                float3 L = normalize(light.direction);
                float3 H = normalize(V + L);

                float NoL = dot(N, L);
                float NoH = dot(N, H);
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


                float4 ilm = tex2D(_ILM, i.uv);

                float matEnum0 = 0.0;
                float matEnum1 = 0.3;
                float matEnum2 = 0.5;
                float matEnum3 = 0.7;
                float matEnum4 = 1.0;

                float ramp0 = _RampMapRow0/10.0 - 0.05;
                float ramp1 = _RampMapRow1/10.0 - 0.05;
                float ramp2 = _RampMapRow2/10.0 - 0.05;
                float ramp3 = _RampMapRow3/10.0 - 0.05;
                float ramp4 = _RampMapRow4/10.0 - 0.05;

                float dayRampV = lerp(ramp4, ramp3, step(ilm.a, (matEnum3 + matEnum4) / 2));
                dayRampV = lerp(dayRampV, ramp2, step(ilm.a, (matEnum2 + matEnum3) / 2));
                dayRampV = lerp(dayRampV, ramp1, step(ilm.a, (matEnum1 + matEnum2) / 2));
                dayRampV = lerp(dayRampV, ramp0, step(ilm.a, (matEnum0 + matEnum1) / 2));
                float nightRampV = dayRampV + 0.5;

                float lambert = max(0, NoL);
                float halflambert = pow(lambert * 0.5 + 0.5, 2);
                float lambertStep = smoothstep(0.423, 0.450, halflambert);

                float rampClampMin = 0.003;
                float rampClampMax = 0.997;

                float rampGrayU = clamp(smoothstep(0.2, 0.4, halflambert), rampClampMin, rampClampMax);
                float2 rampGrayDayUV = float2(rampGrayU, 1 - dayRampV);
                float2 rampGrayNightUV = float2(rampGrayU, 1 - nightRampV);

                float rampDarkU = rampClampMin;
                float2 rampDarkDayUV = float2(rampDarkU, 1 - dayRampV);
                float2 rampDarkNightUV = float2(rampDarkU, 1 - nightRampV);

                float isDay = (L.y + 1) / 2;
                float3 rampGrayColor = lerp(tex2D(_RampTex, rampGrayNightUV).rgb, 
                                                                    tex2D(_RampTex, rampGrayDayUV).rgb, isDay);

                float3 rampDarkColor = lerp(tex2D(_RampTex, rampDarkNightUV).rgb, 
                                                                    tex2D(_RampTex, rampDarkDayUV).rgb, isDay);

                float3 grayShadowColor = baseColor * rampGrayColor * _ShadowColor.rgb;
                float3 darkShadowColor = baseColor * rampDarkColor * _ShadowColor.rgb;

                float3 diffuse = 0;
                diffuse = lerp(grayShadowColor, baseColor, lambertStep);
                diffuse = lerp(darkShadowColor, diffuse, saturate(ilm.g * 2));
                diffuse = lerp(darkShadowColor, diffuse, light.shadowAttenuation);  //投影
                diffuse = lerp(diffuse, baseColor, saturate(ilm.g - 0.5) * 2);

                float blinnPhong = step(0, NoL) * pow(max(0, NoH), _SpecExpon);
                float3 nonMetallicSpec = step(1.04 - blinnPhong, ilm.b) * ilm.r * _KsNonMetallic;
                float3 metallicSpec = blinnPhong * ilm.b * (lambertStep * 0.8 + 0.2) * baseColor * _KsMetallic;

                float isMetal = step(0.95, ilm.r);
                float3 specular = lerp(nonMetallicSpec, metallicSpec, isMetal);
                float3 metallic = lerp(0, tex2D(_MetalTex, matcapUV).r * baseColor, isMetal);

                float3 albedo = diffuse + specular + metallic;

                float rimOffset = 125;
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
                //col.rgb = rim * fresnel;
                clip(col.a - 0.5);

                col.rgb = MixFog(col.rgb, i.fogCoord);
                return col;
            }
            ENDHLSL
        }
    }
}
