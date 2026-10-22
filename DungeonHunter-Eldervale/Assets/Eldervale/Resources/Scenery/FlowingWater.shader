Shader "Eldervale/FlowingWater"
{
    Properties
    {
        _BaseColor("Water tint",Color)=(0.09,0.32,0.34,0.85)
        _Vertical("Waterfall",Float)=0
        _Speed("Current",Float)=1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #define _SCREENSPACEREFLECTIONS_OFF
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 world:TEXCOORD1; };
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _Vertical,_Speed;
            CBUFFER_END
            float _EldervaleTime;
            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 p=v.positionOS.xyz;
                p.z+=sin(p.x*1.7+_EldervaleTime*1.3)*.035*_Vertical;
                p.y+=sin(p.x*.8+_EldervaleTime)*cos(p.z*1.1-_EldervaleTime*.6)*.045*(1-_Vertical);
                o.world=TransformObjectToWorld(p);o.positionCS=TransformWorldToHClip(o.world);o.uv=v.uv;return o;
            }
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float noise(float2 p)
            {
                float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);
            }
            half4 frag(Varyings i):SV_Target
            {
                float t=_EldervaleTime*_Speed;
                float2 uv=lerp(i.world.xz*.5,float2(i.uv.x*24,i.uv.y*8+t*2.5),_Vertical);
                float n=noise(uv+float2(t*.25,-t*.5));
                float fine=noise(uv*4+float2(-t*.3,t*.8));
                float ripple=pow(saturate(sin(uv.y*13+n*4-t*2)*.5+.5),14);
                float foam=smoothstep(.6,.87,n+fine*.22);
                half3 c=lerp(_BaseColor.rgb,_BaseColor.rgb*1.5+float3(.16,.25,.23),n*.6);
                c=lerp(c,float3(.82,.93,.9),saturate(foam*.45+_Vertical*(fine*.5+.2)+ripple*.15));
                half3 normal=normalize(half3(sin(i.world.x*2+t)*.11,1,cos(i.world.z*1.7-t)*.13));
                half3 viewDirection=GetWorldSpaceNormalizeViewDir(i.world);
                half3 reflected=GlossyEnvironmentReflection(reflect(-viewDirection,normal),.16h,1.0h,true);
                float fresnel=.18+pow(1-saturate(dot(normal,viewDirection)),5)*.65;
                Light sun=GetMainLight();float sparkle=pow(saturate(dot(reflect(-sun.direction,normal),viewDirection)),100);
                c=lerp(c,reflected,saturate(fresnel)*(1-_Vertical))+sun.color*sparkle*(1-_Vertical)*2.5;
                float edge=smoothstep(0,.055,i.uv.x)*smoothstep(0,.055,1-i.uv.x);
                return half4(c,_BaseColor.a*lerp(1,edge,_Vertical));
            }
            ENDHLSL
        }
    }
}
