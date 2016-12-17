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
    float4 _CamerasPos0; //Inlined array of camera positions with preset count of 16.
    float4 _CamerasPos1;
    float4 _CamerasPos2;
    float4 _CamerasPos3;
    float4 _CamerasPos4;
    float4 _CamerasPos5;
    float4 _CamerasPos6;
    float4 _CamerasPos7;
    float4 _CamerasPos8;
    float4 _CamerasPos9;
    float4 _CamerasPos10;
    float4 _CamerasPos11;
    float4 _CamerasPos12;
    float4 _CamerasPos13;
    float4 _CamerasPos14;
    float4 _CamerasPos15;

    v2f vert(appdata_t v)
    {
        v2f o;
        o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
        return o;
    }

    float4 raycastColor(float2 t0, float2 t1, float contribution)
    {
        int iterations = 8;

        float4 c1;
        float2 dt = (t1 - t0) / iterations; //step size
        float solidBlocking = 40 * length(dt); // light blocking coeficient of solids
        float airBlocking = 1 * length(dt); // light blocking coeficient of air
        float2 t = t0;
        float4 c0 = fixed4(0.0, 0.0, 0.0, 0.0);   //initially transparent

        for(int i = 0; i < iterations; ++i)
        {
            c1 = tex2D(_MainTex, t);

            c0.a += (c1.a*c1.a*solidBlocking) + airBlocking; //add opacity

            t += dt;
        }
        
        return c0 + (1.0 - c0) * (1.0 - contribution);
    }
    
    fixed4 frag(v2f input) : SV_Target
    {
        /* 
        //this should be working, but does not because you can't pass a single array element to the shader for some reason.
        // would require passing whole array on every change

        int iterations = 8;
        float2 t1 = _PlayerPos.xy;       // texture end point (player position) <0,1>
        float2 t0 = input.texcoord;          // texture start point <0,1>

        float4 c1;
        float2 dt = (t1 - t0) / iterations;
        float solidBlocking = 40 * length(dt); // light blocking coeficient of solids
        float airBlocking = 2 * length(dt); // light blocking coeficient of air
        float2 t = t0;
        float4 c0 = fixed4(0.0, 0.0, 0.0, 0.0);   //initially transparent
        float4 col;
        col = min(col, raycastColor(input.texcoord, _PlayerPos.xy, 1.0));

        for(int i = 0; i < 16; ++i)
        {
            col = min(col, raycastColor(input.texcoord, _CamerasPos[i].xy, _CamerasPos[i].z));
        }
        */
        float4 col = float4(0.0, 0.0, 0.0, 1.0); 
        col = min(col, raycastColor(input.texcoord, _PlayerPos.xy, 1.0));
        //inlined due to problems with accessing uniform arrays
        //z coordinates of _CamerasPosX stores information about camera state
        //w is currently unused
        col = min(col, raycastColor(input.texcoord, _CamerasPos0.xy, _CamerasPos0.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos1.xy, _CamerasPos1.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos2.xy, _CamerasPos2.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos3.xy, _CamerasPos3.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos4.xy, _CamerasPos4.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos5.xy, _CamerasPos5.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos6.xy, _CamerasPos6.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos7.xy, _CamerasPos7.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos8.xy, _CamerasPos8.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos9.xy, _CamerasPos9.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos10.xy, _CamerasPos10.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos11.xy, _CamerasPos11.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos12.xy, _CamerasPos12.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos13.xy, _CamerasPos13.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos14.xy, _CamerasPos14.z));
        col = min(col, raycastColor(input.texcoord, _CamerasPos15.xy, _CamerasPos15.z));

        return col;
    }
        ENDCG
    }
    }

}