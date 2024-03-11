 
using Unity.Burst;
using Unity.Entities;

public partial struct  AnimationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
         
    }

    public void OnDestroy(ref SystemState state)
    {
        
    }

    public void OnUpdate(ref SystemState state)
    {
        new AnimationJob()
        {
            delaTime = state.WorldUnmanaged.Time.DeltaTime,
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct AnimationJob : IJobEntity
    {
        public float delaTime;

        private void Execute(in AnimationShareData animationShareData,ref AnimationFrameIndex animationFrameIndex)
        {
            float newIndex = animationFrameIndex.Value + delaTime * animationShareData.frameRate;

            while (newIndex > animationShareData.frameMaxindex)
            {
                if (newIndex > animationShareData.frameMaxindex) newIndex -= animationShareData.frameMaxindex;
            }

            animationFrameIndex.Value = newIndex;
        }
    }
}