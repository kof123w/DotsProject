using Unity.Entities;

public struct AnimationShareData : ISharedComponentData
{
    public float frameRate;
    public int frameMaxindex;
}