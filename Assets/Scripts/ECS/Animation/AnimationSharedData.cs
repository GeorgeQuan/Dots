using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
public struct AnimationSharedData:ISharedComponentData
{
    public float frameRate;//帧率
    public int frameCount;//帧数量
}
