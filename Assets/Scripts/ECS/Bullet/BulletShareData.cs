using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public  struct BulletShareData : ISharedComponentData
{
    public float MoveSpeed;
    public float DestroyTime;
    public float2 colliderOffset;
    public float3 colliderHalfExtents;

}
