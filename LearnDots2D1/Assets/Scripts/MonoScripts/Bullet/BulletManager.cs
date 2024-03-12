using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine; 

public class BulletManager : UnitySingleton<BulletManager>
{
    private List<Bullet> m_flyingBullets = new List<Bullet>();   //激活的子弹列表
    private Stack<Bullet> m_bulletPool = new Stack<Bullet>();   //子弹对象池
    private Transform m_poolTrans;
    private Transform m_worldTrans;
    private int m_maxCreateCount = 5000;  //最多创建的对象个数
    private int m_curCreateCount = 0;
    private float m_bulletActTime = 4.0f;
    [HideInInspector] public GameObject BulletPrafab;
    [HideInInspector] public float BulletSpeed = 8.0f;
    
    private void Update()
    {
        Bullet bulletItem;
        float d = Time.deltaTime;
        for (int i = m_flyingBullets.Count - 1;i >= 0;i--)
        {
            bulletItem = m_flyingBullets[i];
            bulletItem.ActTime -= d;
            if (bulletItem.ActTime <= 0)
            {
                RecoveryBullet(bulletItem);
                m_flyingBullets.RemoveAt(i);
                continue;
            } 
            
            //判断击中
            bool isHit = EnemyManager.Instance.CheckHit(ref bulletItem);
            if (isHit)
            {
                RecoveryBullet(bulletItem);
                m_flyingBullets.RemoveAt(i);
                continue;
            }

            //更新位置
            bulletItem.trans.position += BulletSpeed * d * bulletItem.trans.up;
        }
    }

    public override void Awake()
    {
        GameObject g = new GameObject("BulletPoolRoot");
        m_poolTrans = g.transform;
        
        GameObject g2  = GameObject.Find("WorldPoolRoot");
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
        BulletPrafab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/Prefab/MonoBullet.prefab");
    }

    public void CreateBullet( ref Vector3 position,ref Quaternion rotation)
    { 
        Bullet bullet = Create();
        if (bullet == null){
            return;
        }

        bullet.trans.position = position;
        bullet.trans.rotation = rotation; 
        bullet.ActTime = m_bulletActTime;
        m_flyingBullets.Add(bullet);
    }


    //回收掉子弹
    private void RecoveryBullet(Bullet bullet)
    {
        if (bullet == null)
        {
            return;
        }

       bullet.Hide();
       bullet.ActTime = 0;
       bullet.trans.SetParent(m_poolTrans);
       m_bulletPool.Push(bullet);
    }

    private Bullet Create()
    {
        if (m_bulletPool.Count > 0)
        {
            var bulletPop = m_bulletPool.Pop();
            bulletPop.Show(); 
            bulletPop.ActTime = m_bulletActTime;
            bulletPop.trans.SetParent(m_worldTrans);
            return bulletPop;
        }

        if (m_curCreateCount > m_maxCreateCount)
        {
            return null;
        }

        Bullet bullet = new Bullet(); 
        GameObject p = Instantiate(BulletPrafab);
        bullet.gobj = p;
        bullet.trans = p.transform;
        bullet.ActTime = m_bulletActTime;
        m_curCreateCount++;
        bullet.trans.SetParent(m_worldTrans);
        bullet.Show();
        return bullet;
    }
}

public class  Bullet : GameObjectBase
{ 
    public float ActTime;  //激活时间 
}
