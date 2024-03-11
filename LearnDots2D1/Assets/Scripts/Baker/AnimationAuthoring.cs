 
using UnityEngine;
using Unity.Entities;


public class AnimationAuthoring:MonoBehaviour
{
    public float frameRate;
    public int frameCount;
    
    public class AnimationBaker : Baker<AnimationAuthoring>
    {
        public override void Bake(AnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<AnimationFrameIndex>(entity);
            
            AddSharedComponent<AnimationShareData>(entity,new AnimationShareData()
            {
                frameRate = authoring.frameRate,
                frameMaxindex = authoring.frameCount
            });
        }
    }
}