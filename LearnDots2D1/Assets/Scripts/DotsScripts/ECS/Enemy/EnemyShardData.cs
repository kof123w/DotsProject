using Unity.Entities;
using Unity.Mathematics;

public struct EnemyShardData : ISharedComponentData
{
    public float MoveSpeed;
    public float2 Scale;
}