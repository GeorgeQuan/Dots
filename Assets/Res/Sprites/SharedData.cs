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
    public static readonly SharedStatic<float2> PlayerPos = SharedStatic<float2>.GetOrCreate<keyClass2>();//�������
    public struct keyClass1{};//�����
    public struct keyClass2{};//�����
    public struct keyClass3{};//�����
}
/// <summary>
/// ���ɹ���Ĺ�������
/// </summary>
public struct EnemyShardInfo
{
    public int DeadCounter;//��������������
    public float SpawnInterval;//ˢ�ּ��
    public int SpawnCount;//��������
    public bool PlayHitAudio;//��ǰ�Ƿ�Ҫ����
    public double PlayHitAudioTime;//�̵�ʱ��
}
