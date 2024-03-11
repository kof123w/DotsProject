 
using UnityEngine;
using Unity.Entities;
public class GameManagerAuthoring : MonoBehaviour
{
    public GameObject BulletPrefab;
    public GameObject EnemyPrefab;
    
    public class GameManagerBaker : Baker<GameManagerAuthoring>
    {
        public override void Bake(GameManagerAuthoring authoring)
        {
            //var entity = GetEntity(TransformUsageFlags.Dynamic);
            var entity = GetEntity(TransformUsageFlags.None);
            GameConfigData configData = new GameConfigData();
            configData.BulletPortotype =  GetEntity(authoring.BulletPrefab,TransformUsageFlags.Dynamic);
            configData.EnemyPortotype =  GetEntity(authoring.EnemyPrefab,TransformUsageFlags.Dynamic);
            
            AddComponent<GameConfigData>(entity,configData);
        }
    }
}