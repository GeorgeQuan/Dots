using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public static class SharedData
{
    public static readonly SharedStatic<Entity> singtonEntity = SharedStatic<Entity>.GetOrCreate<keyClass1>();
    public static readonly SharedStatic<EnemyShardInfo> GameSharedData = SharedStatic<EnemyShardInfo>.GetOrCreate<keyClass3>();
    public static readonly SharedStatic<float2> PlayerPos = SharedStatic<float2>.GetOrCreate<keyClass2>();//玩家坐标
    public struct keyClass1{};//定义键
    public struct keyClass2{};//定义键
    public struct keyClass3{};//定义键
}
/// <summary>
/// 生成怪物的共享数据
/// </summary>
public struct EnemyShardInfo
{
    public int DeadCounter;//敌人死亡计数器
    public float SpawnInterval;//刷怪间隔
    public int SpawnCount;//生成数量
    public bool PlayHitAudio;//当前是否要播放
    public double PlayHitAudioTime;//吞掉时间
}
