using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class EnemyAuthoring : MonoBehaviour
{
    public Vector3 Scale = Vector3.one;
    public float MoveSpeed = 4;
    
    public class EnemyBaker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<RendererSortTag>(entity);
            SetComponentEnabled<RendererSortTag>(entity,true);
            
            AddComponent<EnemyData>(entity);
            SetComponentEnabled<EnemyData>(entity,true);
            AddSharedComponent<EnemyShardData>(entity,new EnemyShardData
            {
                MoveSpeed = authoring.MoveSpeed,
                Scale = (Vector2)authoring.transform.localScale,
            });
            
            Debug.Log("查看敌人烘培的频率=。=");
        }
    }
}
