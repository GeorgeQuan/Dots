using Unity.Entities;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    
    public float MoveSpeed = 4;
    public class EnemyBaker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity eneity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<RendererSortTag>(eneity);
            SetComponentEnabled<RendererSortTag>(eneity, true);

            AddComponent<EnemyData>(eneity, new EnemyData() { Die = false });
            SetComponentEnabled<EnemyData>(eneity, true);

            AddSharedComponent<EnemySharedData>(eneity, new EnemySharedData()
            {
                MoveSpeed=authoring.MoveSpeed,
                scale=(Vector2)authoring.transform.localScale,//v2 和float2 可以做隐式转换
            });

        }
    }
}
