using UnityEngine;

public class EnemyState : MonoBehaviour
{
    public bool IsDie = false;

    public void SetDead()
    {
        IsDie = true;
    }
}