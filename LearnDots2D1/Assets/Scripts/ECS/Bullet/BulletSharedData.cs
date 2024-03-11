using Unity.Entities;
using Unity.Mathematics;
public struct BulletShardData : ISharedComponentData
{
    public float MoveSpeed;
    public float DestroyTimer;

    public float2 colliderOffset;
    public float3 colliderHalfExtents;
}