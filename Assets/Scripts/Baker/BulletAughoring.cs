using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BulletAughoring : MonoBehaviour
{
    public float MoveSpeed;
    public float DestroyTime;

    public class BulletBaker : Baker<BulletAughoring>
    {
        public override void Bake(BulletAughoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<RendererSortTag>(entity);
            SetComponentEnabled<RendererSortTag>(entity, true);
            AddComponent<BulletData>(entity, new BulletData()
            {
                DesTroyTimer=authoring.DestroyTime,

            });
            SetComponentEnabled<BulletData>(entity, true);
            Vector2 collidersize=authoring.GetComponent<BoxCollider2D>().size/2;
            AddSharedComponent<BulletShareData>(entity, new BulletShareData()
            {
                MoveSpeed = authoring.MoveSpeed,
                DestroyTime = authoring.DestroyTime,
                colliderOffset=authoring.GetComponent<BoxCollider2D>().offset,
                colliderHalfExtents=new float3(collidersize.x, collidersize.y,10000),

            });

        }
    }
}
