using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;

public partial struct BulletSystem : ISystem
{
    public readonly static SharedStatic<int> CreateBulletCount = SharedStatic<int>.GetOrCreate<BulletSystem>();//�̰߳�ȫ����̬����

    public void OnCreate(ref SystemState state)
    {
        CreateBulletCount.Data = 0;
        SharedData.singtonEntity.Data = state.EntityManager.CreateEntity(typeof(BulletCreateInfo));
        //������һ���µ�ʵ�壬����Ϊ���ʵ�������һ�� BulletCreateInfo �����������
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();// AsParallelWriter  ����д��汾



        DynamicBuffer<BulletCreateInfo> bulletCreateInfoBuffer = SystemAPI.GetSingletonBuffer<BulletCreateInfo>();//��ȡ�������
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
        state.CompleteDependency();//�ȴ�jobִ����

        if (CreateBulletCount.Data > 0)
        {
            //���䲻��Ĳ���

            //if (bulletCreateInfoBuffer.Length == 0) return;//�ж��Ƿ����ӵ�
            using NativeArray<Entity> newBullets = new NativeArray<Entity>(CreateBulletCount.Data, Allocator.Temp);//����ԭ������,��ʱ������
            ecb.Instantiate(int.MinValue, SystemAPI.GetSingleton<GameConfigData>().BulletPortotype, newBullets);
            //state.EntityManager.Instantiate(SystemAPI.GetSingleton<GameConfigData>().BulletPortotype, newBullets);//����ʵ��
            for (int i = 0; i < newBullets.Length; i++)//�������,��������ݸ�ֵ���´�����ʵ��
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
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]//����������������Ǽ���״̬���ǷǼ���״̬
    [BurstCompile]
    public partial struct BulletJob : IJobEntity
    {
        public uint enemyLayerMask;
        public EntityCommandBuffer.ParallelWriter ecb;
        public float Delatime;
        [ReadOnly] public DynamicBuffer<BulletCreateInfo> bulletCreateInfoBuffer;//ֻ���Ӷ�ʵ���̰߳�ȫ
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
            //��ǰ�ӵ��ǷǼ���״̬,ͬʱ��Ҫ�����ӵ�
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
                //������Ҫ�Ӷ������ȡ��
                return;
            }


            localTransform.Position += bulletShareData.MoveSpeed * Delatime * localTransform.Up();//λ���ƶ�

            bulletData.DesTroyTimer -= Delatime;//���ټ�ʱ
            if (bulletData.DesTroyTimer <= 0)
            {
                //ecb.DestroyEntity(entity.Index, entity);
                bulletEnableState.ValueRW = false;
                sortEnableState.ValueRW = false;
                localTransform.Scale = 0;
                return;

            }
            //�˺����
            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = enemyLayerMask, // all 1s, so all layers, collide with everything
                GroupIndex = 0//Ĭ���������˶��ܷ�����ײ
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
