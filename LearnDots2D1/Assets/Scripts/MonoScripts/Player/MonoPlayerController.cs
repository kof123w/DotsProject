using System;
using System.Collections;
using System.Collections.Generic;
using Mono;
using Unity.Entities;
using UnityEngine;
 

public class MonoPlayerController : MonoBehaviour
{
    [SerializeField] public Animator  M_Animator;
    [SerializeField] public float MoveSpeed;
    [SerializeField] public Vector2 MoveRangeLeftBottom;
    [SerializeField] public Vector2 MoveRangeRightTop;
    [SerializeField] public Transform GunRoot;
    [SerializeField] private int Lv = 1;
    
    public int BulletQuantity
    {
        get => Lv;
    }

    public float AttackCD
    {
        get => Mathf.Clamp(1f / Lv * 1.5f, 0.1f, 1f);
    }

    public int GetLv
    {
        get => Lv;
        set
        {
            Lv = value; 
            EnemyManager.Instance.SetSpawnInterval = 10f / Lv * SpawnMonsterIntervalMultiply;
            EnemyManager.Instance.SetSpawnCount = (int) (Lv * 5 * SpawnMonsterQuantityMultiply);
            EnemyManager.Instance.PlayerPos = m_trans.position;
        }
        
    }

    public float SpawnMonsterIntervalMultiply = 1;
    public float SpawnMonsterQuantityMultiply = 1;

    private PlayerState PlayerCurState;

    [HideInInspector] private Transform m_trans;
    [HideInInspector] private GameObject m_gObj;
    public PlayerState PlayerState
    {
        get { return PlayerCurState; }
        set
        {
            if (value == PlayerCurState)
            {
                return;
            }

            PlayerCurState = value;
            switch (PlayerCurState)
            {
                case PlayerState.Idle:
                    PlayAnimation("Idel");
                    break;
                case PlayerState.Move:
                    PlayAnimation("Move");
                    break;
            }
        }
    }

    private void Awake()
    {
        m_gObj = gameObject;
        m_trans = transform;
        GetLv = Lv;  
        if (M_Animator == null)
        {
            M_Animator = m_trans.Find("View").GetComponent<Animator>();
        } 
    }

    private void Start()
    {
        PlayerCurState = PlayerState.Idle;
    }

    private void Update()
    {
        CheckAttack();
        CheckMove(); 
    }

    private void CheckMove()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool isMoveState = h != 0 || v != 0;
        PlayerState = isMoveState ? PlayerState.Move : PlayerState.Idle;
        if (isMoveState)
        {
            Vector3 finalPos = m_trans.position + MoveSpeed * Time.deltaTime * new Vector3(h, v, 0);
            CheckPositionRange(ref finalPos);
            m_trans.position = finalPos;
            m_trans.localScale = h > 0 ? Vector3.one :new Vector3(-1,1,1);
            EnemyManager.Instance.PlayerPos = m_trans.position;  //每次更新位置在敌人的管理器处保存玩家的坐标信息，便于生成怪物
        }
    }

    private void CheckPositionRange(ref Vector3 pos)
    { 
        pos.x = Mathf.Clamp(pos.x,MoveRangeLeftBottom.x,MoveRangeRightTop.x);
        pos.y = Mathf.Clamp(pos.y, MoveRangeLeftBottom.y, MoveRangeRightTop.y);
        pos.z = pos.y;
    }


    public void PlayAnimation(string animationName)
    {
        M_Animator.CrossFadeInFixedTime(animationName,0);
    }

    private float AttackCDTimer;

    private void CheckAttack()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GunRoot.up = (Vector2) mousePos - (Vector2) m_trans.position ;
        AttackCDTimer -= Time.deltaTime;
        if (AttackCDTimer <= 0 && Input.GetMouseButton(0))
        {
            Attack();
            AttackCDTimer = AttackCD;
        }
    }

    private void Attack()
    {
        AudioManager.instance.PlayShoot();

        Vector3 pos = GunRoot.position;
        Quaternion rot = GunRoot.rotation;
        //生成子弹信息 
        BulletManager.Instance.CreateBullet(ref pos,ref rot);
       
        float angleStep = Mathf.Clamp(360 / BulletQuantity, 0, 5f);
        for (int i = 1; i < BulletQuantity / 2; i++)
        {
            rot = GunRoot.rotation * Quaternion.Euler(0, 0, angleStep * i);
            BulletManager.Instance.CreateBullet(ref pos,ref rot);
            
            rot = GunRoot.rotation * Quaternion.Euler(0, 0, -angleStep * i);
            BulletManager.Instance.CreateBullet(ref pos,ref rot); 
        }
    }
}
