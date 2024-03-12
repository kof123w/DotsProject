using System.Collections.Generic;
using ProjectDawn.Navigation;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Unity.Physics;
using Unity.VisualScripting;
using Random = Unity.Mathematics.Random;


public class EnemyManager : UnitySingleton<EnemyManager>
{
    public float SetSpawnInterval
    {
        set { m_spawnInterval = value; }
    }

    public float SetSpawnCount
    {
        set { m_spawnCount = value; }
    }

    private float m_spawnInterval;
    private float m_spawnCount;
    private float m_curSpawnTime = 0.0f;

    private float m_radiusPow = 0.1f; 
    
    private List<Enemy> m_actMonsterList = new List<Enemy>();
    private Stack<Enemy> m_enemyPool = new Stack<Enemy>();
    [SerializeField] private float MoveSpeed = 1.5f;
    private Transform m_enemyPoolTrans;
    private Transform m_worldTrans;
    private int m_maxCreateCount = 5000; //最多创建的对象个数
    private int m_curCreateCount = 0;
    private GameObject EnemyPrefab;
    public Vector3 PlayerPos;
    private Random m_random = new Random((uint)System.DateTime.Now.GetHashCode());

    public override void Awake()
    {
        GameObject g = new GameObject("EnemyPoolRoot");
        m_enemyPoolTrans = g.transform;

        GameObject g2 = GameObject.Find("WorldPoolRoot");
        if (g2 != null)
        {
            m_worldTrans = g2.transform;
        }

        if (m_worldTrans == null)
        {
            g2 = new GameObject("WorldPoolRoot");
            m_worldTrans = g2.transform;
        }

        //没有资源管理器暴力加载资源
        EnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/Prefab/MonoBee.prefab");
    } 

    private void CheckDir(ref Enemy enemy)
    {
        //判断玩家坐标
        enemy.trans.localScale = enemy.trans.position.x > PlayerPos.x
            ? new Vector3(0.37f, 0.39f, 0f)
            : new Vector3(-0.37f, 0.39f, 0f);
    }

    private void Update()
    {
        float d = Time.deltaTime;
        m_curSpawnTime += d;

        if (m_curSpawnTime >= m_spawnInterval)
        {
            m_curSpawnTime = 0;
            RandomCreateEnemy();
        }

        CheckMove(ref d);
    }

    private Agent _agent;
    private void CheckMove(ref float d) 
    {
        Enemy enemy;
        int len = m_actMonsterList.Count;
        for (int i = len - 1;i>=0;i--)
        {
            enemy = m_actMonsterList[i];
            if(enemy == null) continue;
            if (enemy.GetIsDie())
            {
                RecoveryEnemy(enemy);
                m_actMonsterList.RemoveAt(i);
                continue; 
            }

            var dir = PlayerPos - enemy.trans.position;
            enemy.trans.position += d * dir.normalized * MoveSpeed;
            CheckDir(ref enemy);
        }
    }

    private void RandomCreateEnemy()
    {
        //创建
        for (int i = 0; i < m_spawnCount; i++)
        {
            var enemy = Create();
            if (enemy == null)
            {
                continue;
            }
            
            float2 offset = m_random.NextFloat2Direction() * m_random.NextFloat2(5f, 10);
            enemy.trans.position = new Vector3(offset.x + PlayerPos.x, offset.y + PlayerPos.y, offset.y + PlayerPos.y);
            CheckDir(ref enemy);
            m_actMonsterList.Add(enemy);
        }
    }

    private Enemy Create()
    {
        if (m_enemyPool.Count > 0)
        {
            var enemyPop = m_enemyPool.Pop();
            enemyPop.Show();
            enemyPop.trans.SetParent(m_worldTrans);
            enemyPop.Init();
            return enemyPop;
        }

        if (m_curCreateCount > m_maxCreateCount)
        {
            return null;
        }

        Enemy enemy = new Enemy();
        GameObject p = Instantiate(EnemyPrefab);
        enemy.gobj = p;
        enemy.trans = p.transform;
        m_curCreateCount++;
        enemy.trans.SetParent(m_worldTrans);
        enemy.Show();
        enemy.Init();
        return enemy;
    }

    private void RecoveryEnemy(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.Hide();
        enemy.trans.SetParent(m_enemyPoolTrans);
        m_enemyPool.Push(enemy); 
    }
     
    //传子弹进来计算击中的怪物
    public bool CheckHit(ref Bullet bullet)
    {
        Vector3 pos = bullet.trans.position;
        Vector3 enemyPos;
        Enemy enemy;
        for (int i = 0;i < m_actMonsterList.Count;i++)
        {
            enemy = m_actMonsterList[i];
            enemyPos = enemy.trans.position;
            if (GetDisPow(ref pos,ref enemyPos) <= m_radiusPow)
            {
                enemy.Dead();
                return true;
            }
        }

        return false;
    }

    public float GetDisPow(ref Vector3 bullPos,ref Vector3 enemyPos)
    {
        return (bullPos.x - enemyPos.x) * (bullPos.x - enemyPos.x) + 
               (bullPos.y - enemyPos.y) * (bullPos.y - enemyPos.y) ;
    }
}

class Enemy : GameObjectBase
{ 
    private bool IsDie;
    public void Init()
    { 
        IsDie = false;
    }

    public void Dead()
    {
        IsDie = true;
    }

    public bool GetIsDie()
    {
        return IsDie;
    }
}