void SpherizeNormal_float(float2 ObjectPos, float2 Center, float Radius, out float3 OutNormal)
{
    float2 localPos = ObjectPos - Center;
    float2 normalizedPos = localPos / Radius;
    float distSq = dot(normalizedPos, normalizedPos);
    if (distSq > 1.0) 
    {
        normalizedPos = normalizedPos / sqrt(distSq);
        distSq = 1.0;
    }
    float z = sqrt(max(0.0, 1.0 - distSq));
    OutNormal = normalize(float3(normalizedPos.x * 0.4, normalizedPos.y * 0.4, z));
}