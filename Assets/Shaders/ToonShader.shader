Shader "Custom/ToonShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow

        fixed4 _BaseColor;

        struct Input
        {
            float3 worldNormal;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Use the base color
            fixed4 baseColor = _BaseColor;

            // Compute lighting
            half3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
            half nl = dot(IN.worldNormal, worldLightDir);

            // Simple toon shading step function
            half toonShade = step(0.5, nl);

            // Apply lighting and base color
            o.Albedo = baseColor.rgb * toonShade;
            o.Alpha = baseColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
