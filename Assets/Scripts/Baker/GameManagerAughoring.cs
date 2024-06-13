using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

public class GameManagerAughoring : MonoBehaviour
{
    public GameObject BulletPrefab;//子弹预制体
    public GameObject EnemyPrefab;//敌人预制体
    public class GameManagerBaker : Baker<GameManagerAughoring>
    {
        public override void Bake(GameManagerAughoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            GameConfigData configData = new GameConfigData();
            configData.BulletPortotype = GetEntity(authoring.BulletPrefab, TransformUsageFlags.Dynamic);
            configData.EnemyPortotype = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic);
            AddComponent<GameConfigData>(entity, configData);
        }
    }
}
