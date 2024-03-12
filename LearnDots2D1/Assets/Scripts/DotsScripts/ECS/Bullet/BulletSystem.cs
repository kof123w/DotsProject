using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;

public partial struct BulletSystem : ISystem
{
    public readonly static SharedStatic<int> CreateBulletCount = SharedStatic<int>.GetOrCreate<BulletSystem>();

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameConfigData>(); 
        CreateBulletCount.Data = 0;
        ShareData.singleEntity.Data = state.EntityManager.CreateEntity(typeof(BulletCreateInfo));
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer.ParallelWriter ecb = SystemAPI
            .GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter(); 
        
        DynamicBuffer<BulletCreateInfo> bulletCreateInfos = SystemAPI.GetSingletonBuffer<BulletCreateInfo>();
        CreateBulletCount.Data = bulletCreateInfos.Length;
        
        new BulletJob()
        { 
            EnemyLayerMask = 1<<6,
            ecb = ecb,
            deltaTime = SystemAPI.Time.DeltaTime,
            BulletCreateInfos = bulletCreateInfos,
            CollisionWorldParam = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
            //EnemyLookup =  SystemAPI.GetComponentLookup<EnemyData>()
        }.ScheduleParallel();
        state.CompleteDependency(); 
        if (CreateBulletCount.Data > 0)
        {
            //补充对象池
            NativeArray<Entity> newBullets = new NativeArray<Entity>(CreateBulletCount.Data,Allocator.Temp);
            //state.EntityManager.Instantiate(SystemAPI.GetSingleton<GameConfigData>().BulletPortotype,newBullets);
            ecb.Instantiate(int.MinValue, SystemAPI.GetSingleton<GameConfigData>().BulletPortotype, newBullets);
            for (int i = 0;i<newBullets.Length;i++)
            {
                BulletCreateInfo info = bulletCreateInfos[i];
                ecb.SetComponent<LocalTransform>(newBullets[i].Index,newBullets[i],new LocalTransform()
                {
                    Position = info.position,
                    Rotation = info.rotation,
                    Scale = 1
                });
            }
            newBullets.Dispose();
        } 
        
        bulletCreateInfos.Clear();
        
    }
    
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    [BurstCompile]
    public partial struct BulletJob : IJobEntity
    {
        public uint EnemyLayerMask;
        public EntityCommandBuffer.ParallelWriter ecb;
        public float deltaTime;
        [ReadOnly] public DynamicBuffer<BulletCreateInfo> BulletCreateInfos;
        [ReadOnly] public CollisionWorld CollisionWorldParam;
       // [ReadOnly] public ComponentLookup<EnemyData> EnemyLookup;
        public void Execute(EnabledRefRW<BulletData> bulletEnableState,EnabledRefRW<RendererSortTag> rendererSortTagEnabledRefRw,
            ref BulletData bulletData,in BulletShardData bulletShardData,in Entity  entity,ref LocalTransform localTransform)
        {
            //创建子弹大于0，且关闭状态就取消休眠
            if (bulletEnableState.ValueRO == false)
            {
                if ( CreateBulletCount.Data > 0)
                {
                    int index = CreateBulletCount.Data-=1;
                    bulletEnableState.ValueRW = true;
                    localTransform.Position = BulletCreateInfos[index].position;
                    localTransform.Rotation = BulletCreateInfos[index].rotation;
                    localTransform.Scale = 1;
                    bulletData.DestroyTimer = bulletShardData.DestroyTimer;
                }

                return;
            }

            localTransform.Position += bulletShardData.MoveSpeed * deltaTime * localTransform.Up();
            
            //销毁计时器
            bulletData.DestroyTimer -= deltaTime;
            if (bulletData.DestroyTimer <= 0)
            {
                //ecb.DestroyEntity(entity.Index,entity);
                bulletEnableState.ValueRW = false;
                rendererSortTagEnabledRefRw.ValueRW = false;
                localTransform.Scale = 0;
                return;
            }
            
            //伤害检查
            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
            CollisionFilter cf = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = EnemyLayerMask,
                GroupIndex = 0
            };
          
            if (CollisionWorldParam.OverlapBox(localTransform.Position,localTransform.Rotation,bulletShardData.colliderHalfExtents,ref hits,cf))
            {
                for (int i = 0;i<hits.Length;i++)
                { 
                    Entity tmp = hits[i].Entity;
                    /*if (EnemyLookup.HasComponent(tmp))
                    {
                        bulletData.DestroyTimer = 0; 
                        ecb.SetComponent<EnemyData>(tmp.Index,tmp,new EnemyData()
                        {
                            Die = true
                        });
                    }*/
                    bulletData.DestroyTimer = 0; 
                    ecb.SetComponent<EnemyData>(tmp.Index,tmp,new EnemyData()
                    {
                        Die = true
                    });
                }
            }

            hits.Dispose();
        } 
    }
}