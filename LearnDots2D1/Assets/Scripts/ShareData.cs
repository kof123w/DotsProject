using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

public static class ShareData
{
    public static readonly SharedStatic<Entity> singleEntity = SharedStatic<Entity>.GetOrCreate<SharedStatic<Entity>>();
    public static readonly SharedStatic<GameShardData> gameSharedData = SharedStatic<GameShardData>.GetOrCreate<SharedStatic<GameShardData>>();
    public static readonly SharedStatic<float2> playerPos = SharedStatic<float2>.GetOrCreate<SharedStatic<keyClass2>>();
    
    public struct keyClass1  { }
    
    public struct keyClass2 { }
}

public struct GameShardData
{
    public int DeadCounter;
    public float SpawnInterval;
    public int SpawnCount;

    public bool playHitAudio;
    public double playHitAudioTime;
}