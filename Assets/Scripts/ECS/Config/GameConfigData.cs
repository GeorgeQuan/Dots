﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public struct  GameConfigData:IComponentData
{
    public Entity BulletPortotype;
    public Entity EnemyPortotype;
}
