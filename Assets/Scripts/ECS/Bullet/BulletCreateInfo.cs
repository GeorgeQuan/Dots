using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public  struct BulletCreateInfo:IBufferElementData
{
    public float3 position;
    public quaternion rotation;
}
