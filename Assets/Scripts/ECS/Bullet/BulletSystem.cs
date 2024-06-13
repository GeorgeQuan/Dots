using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;

public partial struct BulletSystem : ISystem
{
    public readonly static SharedStatic<int> CreateBulletCount = SharedStatic<int>.GetOrCreate<BulletSystem>();//线程安全共享静态数据

    public void OnCreate(ref SystemState state)
    {
        CreateBulletCount.Data = 0;
        SharedData.singtonEntity.Data = state.EntityManager.CreateEntity(typeof(BulletCreateInfo));
        //创建了一个新的实体，并且为这个实体添加了一个 BulletCreateInfo 缓冲区组件。
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();// AsParallelWriter  并行写入版本



        DynamicBuffer<BulletCreateInfo> bulletCreateInfoBuffer = SystemAPI.GetSingletonBuffer<BulletCreateInfo>();//获取容器组件
        CreateBulletCount.Data = bulletCreateInfoBuffer.Length;

        new BulletJob()
        {
            enemyLayerMask = 1 << 6,
            ecb = ecb,
            Delatime = SystemAPI.Time.DeltaTime,
            bulletCreateInfoBuffer = bulletCreateInfoBuffer,
            collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
            // enemyLookUp = SystemAPI.GetComponentLookup<EnemyData>()

        }.ScheduleParallel();
        state.CompleteDependency();//等待job执行完

        if (CreateBulletCount.Data > 0)
        {
            //补充不足的部分

            //if (bulletCreateInfoBuffer.Length == 0) return;//判断是否有子弹
            using NativeArray<Entity> newBullets = new NativeArray<Entity>(CreateBulletCount.Data, Allocator.Temp);//创建原型数组,临时分配器
            ecb.Instantiate(int.MinValue, SystemAPI.GetSingleton<GameConfigData>().BulletPortotype, newBullets);
            //state.EntityManager.Instantiate(SystemAPI.GetSingleton<GameConfigData>().BulletPortotype, newBullets);//创建实例
            for (int i = 0; i < newBullets.Length; i++)//遍历组件,把组件数据赋值给新创建的实体
            {
                BulletCreateInfo info = bulletCreateInfoBuffer[i];
                ecb.SetComponent<LocalTransform>(newBullets[i].Index, newBullets[i], new LocalTransform()
                {
                    
                    Position = info.position,
                    Rotation = info.rotation,
                    Scale = 1
                });

            }
        }



        bulletCreateInfoBuffer.Clear();



    }
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]//遍历所有组件无论是激活状态还是非激活状态
    [BurstCompile]
    public partial struct BulletJob : IJobEntity
    {
        public uint enemyLayerMask;
        public EntityCommandBuffer.ParallelWriter ecb;
        public float Delatime;
        [ReadOnly] public DynamicBuffer<BulletCreateInfo> bulletCreateInfoBuffer;//只读从而实现线程安全
        [ReadOnly] public CollisionWorld collisionWorld;
        //[ReadOnly] public ComponentLookup<EnemyData> enemyLookUp;
        public void Execute(
            EnabledRefRW<BulletData> bulletEnableState,
            EnabledRefRW<RendererSortTag> sortEnableState,
            ref BulletData bulletData,
            ref LocalTransform localTransform,
            in Entity entity,
            in BulletShareData bulletShareData)
        {
            //当前子弹是非激活状态,同时需要创建子弹
            if (bulletEnableState.ValueRO == false)
            {
                if (CreateBulletCount.Data > 0)
                {
                    int index = CreateBulletCount.Data -= 1;
                    bulletEnableState.ValueRW = true;
                    sortEnableState.ValueRW = true;
                    localTransform.Position = bulletCreateInfoBuffer[index].position;
                    localTransform.Rotation = bulletCreateInfoBuffer[index].rotation;
                    localTransform.Scale = 1;
                    bulletData.DesTroyTimer = bulletShareData.DestroyTime;
                }
                //可能需要从对象池中取出
                return;
            }


            localTransform.Position += bulletShareData.MoveSpeed * Delatime * localTransform.Up();//位置移动

            bulletData.DesTroyTimer -= Delatime;//销毁计时
            if (bulletData.DesTroyTimer <= 0)
            {
                //ecb.DestroyEntity(entity.Index, entity);
                bulletEnableState.ValueRW = false;
                sortEnableState.ValueRW = false;
                localTransform.Scale = 0;
                return;

            }
            //伤害检测
            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = enemyLayerMask, // all 1s, so all layers, collide with everything
                GroupIndex = 0//默认与所有人都能发生碰撞
            };

            if (collisionWorld.OverlapBox(localTransform.Position, localTransform.Rotation, bulletShareData.colliderHalfExtents, ref hits, filter))
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    Entity temp = hits[i].Entity;
                    bulletData.DesTroyTimer = 0;
                    ecb.SetComponent<EnemyData>(temp.Index, temp, new EnemyData()
                    {
                        Die = true,

                    });
                }
            }




            hits.Dispose();
        }
    }
}
