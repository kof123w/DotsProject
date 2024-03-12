using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial struct EnemySystem : ISystem
{
    public struct key1 {  }
    public struct key2 {  }
    public struct key3 {  }
    public readonly static SharedStatic<int> CreatedCount = SharedStatic<int>.GetOrCreate<key1>();
    public readonly static SharedStatic<int> CreateCount = SharedStatic<int>.GetOrCreate<key2>();
    public readonly static SharedStatic<Random> Random = SharedStatic<Random>.GetOrCreate<key3>();
    public float SpawnEnemyTimer;
    public const int maxEnemys = 5000;
    
    public void OnCreate(ref SystemState state)
    {
        //state.RequireForUpdate<GameConfigData>();
        CreatedCount.Data = 0;
        CreateCount.Data = 0;
        Random.Data = new Random((uint)System.DateTime.Now.GetHashCode());
        ShareData.gameSharedData.Data.DeadCounter = 0;
    }

    public void OnUpdate(ref SystemState state)
    {
        SpawnEnemyTimer -= SystemAPI.Time.DeltaTime;
        if (SpawnEnemyTimer <= 0)
        {
            SpawnEnemyTimer = ShareData.gameSharedData.Data.SpawnInterval;
            CreateCount.Data += ShareData.gameSharedData.Data.SpawnCount;
        }

        EntityCommandBuffer.ParallelWriter ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        float2 playerPos = ShareData.playerPos.Data;
        new EnemyJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            playerPos = playerPos,
            ecb = ecb,
            time = SystemAPI.Time.ElapsedTime
        }.ScheduleParallel();
        state.CompleteDependency();

        if (CreateCount.Data > 0 && CreatedCount.Data < maxEnemys)
        {
            //补充对象池
            NativeArray<Entity> newEnemy = new NativeArray<Entity>(CreateCount.Data,Allocator.Temp);
           // state.EntityManager.Instantiate(SystemAPI.GetSingleton<GameConfigData>().EnemyPortotype,newEnemy);
           ecb.Instantiate(int.MinValue, SystemAPI.GetSingleton<GameConfigData>().EnemyPortotype, newEnemy);
            for (int i = 0;i<newEnemy.Length && CreatedCount.Data < maxEnemys;i++)
            {
                CreatedCount.Data += 1;
                float2 offset = Random.Data.NextFloat2Direction() * Random.Data.NextFloat2(5f, 10);
                ecb.SetComponent<LocalTransform>(newEnemy[i].Index,newEnemy[i],new LocalTransform()
                {
                    Position = new float3(offset.x+playerPos.x,offset.y+playerPos.y,0),
                    Rotation = Quaternion.identity,
                    Scale = 1,
                });
            }

            CreateCount.Data = 0;
            newEnemy.Dispose();
        }
    }
    
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    [BurstCompile]
    public partial struct EnemyJob : IJobEntity
    {
        public float deltaTime;
        public float2 playerPos;
        public double time;
        public EntityCommandBuffer.ParallelWriter ecb;
        private void Execute(EnabledRefRW<EnemyData> enemyEnabledRefRw,EnabledRefRW<RendererSortTag> sortEnabledRefRw,
            ref EnemyData enemyData,EnabledRefRW<AnimationFrameIndex> animationFrameIndexEnable,
            in EnemyShardData enemyShardData,ref LocalTransform localTransform,ref LocalToWorld localToWorld,ref AgentBody agentBody)
        {
            if (enemyEnabledRefRw.ValueRO == false)
            {
                if (CreateCount.Data > 0)
                {
                    CreateCount.Data -= 1;
                    float2 offset = Random.Data.NextFloat2Direction() * Random.Data.NextFloat2(5f, 10);
                    localTransform.Position = new float3(offset.x+playerPos.x,offset.y+playerPos.y,0);
                    enemyEnabledRefRw.ValueRW = true;
                    sortEnabledRefRw.ValueRW = true;
                    animationFrameIndexEnable.ValueRW = true;
                    localTransform.Scale = 1;
                }

                return;
            }

            if (enemyData.Die)
            {
                //得分
                ShareData.gameSharedData.Data.DeadCounter += 1;
                ShareData.gameSharedData.Data.playHitAudio = true;
                ShareData.gameSharedData.Data.playHitAudioTime = time;
                enemyData.Die = false;
                enemyEnabledRefRw.ValueRW = false;
                sortEnabledRefRw.ValueRW = false;
                animationFrameIndexEnable.ValueRW = false;
                localTransform.Scale = 0;
                //agentBody.Stop();
                return;
            }
            
            agentBody.SetDestination(new float3(playerPos.x,playerPos.y,0));

            /*float x = localTransform.Position.x;
            float y = localTransform.Position.y; 
            var range = playerPos - new float2(x,y);
            if (Mathf.Abs(range.x) < 0.001f)
                return;
            float2 dir = math.normalize(playerPos - new float2(x,y));
            localTransform.Position += deltaTime * enemyShardData.MoveSpeed * new float3(dir.x, dir.y, 0);*/
            localToWorld.Value.c0.x = localTransform.Position.x < playerPos.x
                ? -enemyShardData.Scale.x
                : enemyShardData.Scale.x;
            localToWorld.Value.c1.y = enemyShardData.Scale.y;
        }
    }
}