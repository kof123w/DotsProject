using Unity.Entities;
using Unity.Mathematics;

public struct BulletCreateInfo : IBufferElementData
{
    public float3 position;
    public quaternion rotation;
}