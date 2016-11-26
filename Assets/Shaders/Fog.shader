Shader "Fog" {
    Properties{
        _MainTex("Light Blocking Map", 2D) = "white" {}
        _PlayerPos("Raycast Start Pos", Vector) = (0.06, 0.06, 0, 0)
        _TextureSize("Texture Size", Int) = 16
    }

        SubShader{
        Tags{"Queue" = "Transparent"  "RenderType" = "Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass{
        CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

    struct appdata_t
    {
        float4 vertex : POSITION;
        float2 texcoord : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        half2 texcoord : TEXCOORD0;
    };

    int _TextureSize;
    float4 _PlayerPos;
    sampler2D _MainTex;
    float4 _MainTex_ST;

    v2f vert(appdata_t v)
    {
        v2f o;
        o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
        return o;
    }
    
    fixed4 frag(v2f i) : SV_Target
    {
        //fixed4 col = tex2D(_MainTex, i.texcoord);
        //float4 col = textureBicubic(i.texcoord);
        //return col; 
        
        float solidBlocking = 0.18; // light blocking coeficient of solids <0,1>
        float airBlocking = 0.02; // light blocking coeficient of air <0,1>
        int iterations = 128; //128 gives smooth enough results
        float2 t1 = _PlayerPos.xy;       // texture end point (player position) <0,1>
        float2 t0 = i.texcoord;          // texture start point <0,1>

        float2 t, dt;
        float4 c0, c1;
        dt = normalize(t1 - t0) / iterations;
        c0 = fixed4(0.0, 0.0, 0.0, 0.0);   //initially transparent
        t = t0;

        if(dot(t1 - t, dt) > 0.0)
        {
            for(int j = 0; j<iterations; j++)
            {
                c1 = tex2D(_MainTex, t);

                c0.a += (c1.a*c1.a*solidBlocking) + airBlocking; //add opacity

                if(dot(t1 - t, dt) <= 0.000f) break;
                t += dt;
            }
        }

        fixed4 col = c0;

        return col;
    }
        ENDCG
    }
    }

}