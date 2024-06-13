using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public enum PlayerState
{
    Idle, Move
}
public class PlayerController : MonoBehaviour
{
    public Animator Animator;
    public float MoveSpeed;
    public Vector2 MoveRangeX;
    public Vector2 MoveRangeY;
    public Transform GunRoot;
    public int lv = 1;
    public int BulletQuantity { get => lv; }//子弹数量
    public float AttackCD { get => Mathf.Clamp(1F / Lv * 1.5F, 0.1F, 1F); }//限制攻击CD
    public int Lv
    {
        get => lv;
        set
        {
            lv = value;
            //因为等级导致的初始化数据,这里在设置玩家等级时改变刷怪数据
            SharedData.GameSharedData.Data.SpawnInterval = 10 / lv * SpawnMonsterIntervalMultiply;
            SharedData.GameSharedData.Data.SpawnCount = (int)(lv*5* SpawnMonsterQnantityMultiply);

        }
    }
    public float SpawnMonsterIntervalMultiply = 1;//刷新怪物间隔
    public float SpawnMonsterQnantityMultiply = 1;//刷新怪物数量

    private PlayerState playerState;
    public PlayerState PlayerState
    {
        get { return playerState; }
        set
        {
            playerState = value;
            switch (playerState)
            {
                case PlayerState.Idle:
                    PlayAnimation("Idle");
                    break;
                case PlayerState.Move:
                    PlayAnimation("Move");
                    break;
            }


        }
    }
    private void Awake()
    {
        CheckPositionRange();
        Lv = lv;
    }
    private void Start()
    {
        PlayerState = PlayerState.Idle;//设置默认状态
    }
    private void Update()
    {
        CheckAttack();
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        switch (playerState)
        {
            case PlayerState.Idle:
                if (h != 0 || v != 0) PlayerState = PlayerState.Move;
                break;
            case PlayerState.Move:
                if (h == 0 && v == 0)
                {
                    PlayerState = PlayerState.Idle;
                    return;
                }
                transform.Translate(MoveSpeed * Time.deltaTime * new Vector3(h, v, 0));
                CheckPositionRange();
                if (h > 0) transform.localScale = Vector3.one;
                if (h < 0) transform.localScale = new Vector3(-1, 1, 1);




                break;
        }
    }
    private void CheckPositionRange()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, MoveRangeX.x, MoveRangeX.y);
        pos.y = Mathf.Clamp(pos.y, MoveRangeY.x, MoveRangeY.y);
        pos.z = pos.y;
        transform.position = pos;
        SharedData.PlayerPos.Data = (Vector2)transform.position;//玩家坐标发生变化后,发送给共享组件
    }
    public void PlayAnimation(string animationName)
    {
        Animator.CrossFadeInFixedTime(animationName, 0);
    }
    private float attackCDTimer;
    /// <summary>
    /// 检查攻击方法
    /// </summary>
    private void CheckAttack()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GunRoot.up = new Vector2(mousePos.x, mousePos.y) - new Vector2(transform.position.x, transform.position.y);//萝卜的方向
        attackCDTimer -= Time.deltaTime;
        if (attackCDTimer <= 0 && Input.GetMouseButton(0))//判断攻击
        {
            Attack();
            attackCDTimer = AttackCD;
        }
    }
    /// <summary>
    /// 攻击方法
    /// </summary>
    private void Attack()
    {
        AudioManager.instance.PlayShootAudio();//生成开火的声音
                                               //生成子弹
        DynamicBuffer<BulletCreateInfo> buffer = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<BulletCreateInfo>(SharedData.singtonEntity.Data);//获取共享数据中的缓冲区组件
        //DefaultGameObjectInjectionWorld：Unity 自动创建的默认 World 实例，用于 GameObject 和 ECS 之间的交互。
        buffer.Add(new BulletCreateInfo()
        {
            position = GunRoot.position,
            rotation = GunRoot.rotation,


        });
        float angleStep = Mathf.Clamp(360 / BulletQuantity, 0, 5F);//每颗子弹的角度
        for (int i = 1; i < BulletQuantity / 2; i++)
        {
            buffer.Add(new BulletCreateInfo()
            {
                position = GunRoot.position,
                rotation = GunRoot.rotation * Quaternion.Euler(0, 0, angleStep * i),

            });
            buffer.Add(new BulletCreateInfo()
            {
                position = GunRoot.position,
                rotation = GunRoot.rotation * Quaternion.Euler(0, 0, -angleStep * i),

            });


        }
    }
}
