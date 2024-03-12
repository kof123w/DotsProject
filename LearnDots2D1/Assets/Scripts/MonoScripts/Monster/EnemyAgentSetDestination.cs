using System.Collections;
using System.Collections.Generic;
using ProjectDawn.Navigation;
using ProjectDawn.Navigation.Hybrid;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class EnemyAgentSetDestination : MonoBehaviour
{
    public Transform Target;
    void Start()
    {
        Target = EnemyManager.Instance.PlayerTransform;
        GetComponent<AgentAuthoring>().SetDestination(Target.position);
    }
}

// ECS component
public struct SetDestination : IComponentData
{
    public float3 Value;
}

// Bakes mono component into ecs component
class AgentSetDestinationBaker : Baker<EnemyAgentSetDestination>
{
    public override void Bake(EnemyAgentSetDestination authoring)
    {
        AddComponent(GetEntity(TransformUsageFlags.Dynamic),
            new SetDestination { Value = authoring.Target.position });
    }
}

// Sets agents destination
partial struct AgentSetDestinationSystem : ISystem
{
    public void OnUpdate(ref SystemState systemState)
    {
        foreach (var (destination, body) in SystemAPI.Query<RefRO<SetDestination>, RefRW<AgentBody>>())
        {
            body.ValueRW.SetDestination(destination.ValueRO.Value);
        }
    }
}
