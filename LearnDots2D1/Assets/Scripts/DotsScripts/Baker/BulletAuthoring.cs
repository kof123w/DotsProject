using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BulletAuthoring : MonoBehaviour
{
    public float moveSpeed;
    public float destroyTime;
    
    public class BulletBaker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
           
            AddComponent<BulletData>(entity,new BulletData()
            {
                DestroyTimer = authoring.destroyTime,
            });
            SetComponentEnabled<BulletData>(entity,true); 
            
            AddComponent<RendererSortTag>(entity);
            SetComponentEnabled<RendererSortTag>(entity,true);
            Vector2 colliderSize = authoring.GetComponent<BoxCollider2D>().size / 2f;
            AddSharedComponent<BulletShardData>(entity,new BulletShardData()
            {
                MoveSpeed = authoring.moveSpeed,
                DestroyTimer = authoring.destroyTime,
                colliderOffset = authoring.GetComponent<BoxCollider2D>().offset,
                colliderHalfExtents = new float3(colliderSize.x,colliderSize.y,100000f)
            });
        }
    }
}