using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


public partial struct EnemySystem : ISystem
{
    public struct Key1 { };
    public struct Key2 { };
    public struct Key3 { };
    public readonly static SharedStatic<int> CreatedCount = SharedStatic<int>.GetOrCreate<Key1>();//创建过的敌人数量
    public readonly static SharedStatic<int> CreateCount = SharedStatic<int>.GetOrCreate<Key2>();//需要创建的敌人数量
    public readonly static SharedStatic<Random> random = SharedStatic<Random>.GetOrCreate<Key3>();//随机偏移量


    public float SpawnEnemyTiemr;//定义计时器
    public const int MaxEnemyCount = 10000;//敌人数量
    public void OnCreate(ref SystemState state)
    {
        SpawnEnemyTiemr = 0;
        CreatedCount.Data = 0;
        CreateCount.Data = 0;
        random.Data = new Random((uint)System.DateTime.Now.GetHashCode());
        SharedData.GameSharedData.Data.DeadCounter = 0;//初始化积分数据
    }
    public void OnUpdate(ref SystemState state)
    {
        SpawnEnemyTiemr -= SystemAPI.Time.DeltaTime;
        if (SpawnEnemyTiemr <= 0)
        {
            SpawnEnemyTiemr = SharedData.GameSharedData.Data.SpawnInterval;
            CreateCount.Data += SharedData.GameSharedData.Data.SpawnCount;
        }
        EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        float2 playerPos = SharedData.PlayerPos.Data;
        new EnemyJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            playerPos = playerPos,
            time=SystemAPI.Time.ElapsedTime,
            ecb = ecb,
        }.ScheduleParallel(); //Run(), Schedule();
        state.CompleteDependency();



        if (CreateCount.Data > 0 && CreatedCount.Data < MaxEnemyCount)//判断生成条件是否满足
        {
            NativeArray<Entity> newEnemys = new NativeArray<Entity>(CreateCount.Data, Allocator.Temp);
            ecb.Instantiate(int.MinValue, SystemAPI.GetSingleton<GameConfigData>().EnemyPortotype, newEnemys);
            for (int i = 0; i < newEnemys.Length && CreatedCount.Data < MaxEnemyCount; i++)
            {
                CreatedCount.Data += 1;
                float2 offset = random.Data.NextFloat2Direction() * random.Data.NextFloat(5f, 10);
                ecb.SetComponent<LocalTransform>(newEnemys[i].Index, newEnemys[i], new LocalTransform()
                {
                    Position = new float3(playerPos.x + offset.x, playerPos.y + offset.y, 0),
                    Rotation = quaternion.identity,
                    Scale = 1

                });
            }
            CreateCount.Data = 0;
            newEnemys.Dispose();

        }
    }
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    [BurstCompile]
    public partial struct EnemyJob : IJobEntity
    {
        public float deltaTime;
        public double time;
        public float2 playerPos;
        public EntityCommandBuffer.ParallelWriter ecb;
        private void Execute(EnabledRefRW<EnemyData> enableState, ref LocalToWorld localToWorld, EnabledRefRW<RendererSortTag> rendererSortEnableState, EnabledRefRW<AnimationFrameIndex> animationEnableState, ref EnemyData enemyData, in EnemySharedData enemySharedData, ref LocalTransform localTransform)
        {
            if (enableState.ValueRO == false)//判断当前是否有为激活的敌人实体
            {
                if (CreateCount.Data > 0)
                {
                    CreateCount.Data -= 1;//需要生成的实体数量-1 //下面是激活敌人的组件
                    float2 offset = random.Data.NextFloat2Direction() * random.Data.NextFloat(5f, 10);//NextFloat2Direction随机方向
                    localTransform.Position = new float3(playerPos.x + offset.x, playerPos.y + offset.y, 0);
                    enableState.ValueRW = true;
                    rendererSortEnableState.ValueRW = true;
                    animationEnableState.ValueRW = true;
                    localTransform.Scale = 1;
                }
                return;
            }
            if (enemyData.Die)
            {
                SharedData.GameSharedData.Data.PlayHitAudio = true;//死亡
                SharedData.GameSharedData.Data.DeadCounter += 1;//得分
                SharedData.GameSharedData.Data.PlayHitAudioTime=time;
                enemyData.Die = false;
                enableState.ValueRW = false;
                rendererSortEnableState.ValueRW = false;
                animationEnableState.ValueRW = false;
                localTransform.Scale = 0;
                return;
            }
            float2 dir = math.normalize(playerPos - new float2(localTransform.Position.x, localTransform.Position.y));
            localTransform.Position += deltaTime * enemySharedData.MoveSpeed * new float3(dir.x, dir.y, 0);
            localToWorld.Value.c0.x = localToWorld.Position.x < playerPos.x ? -enemySharedData.scale.x : enemySharedData.scale.x;//判断敌人朝向  这里设置的是矩阵
            localToWorld.Value.c1.y = enemySharedData.scale.y;
        }
    }
}
