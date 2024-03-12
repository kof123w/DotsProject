using System;
using UnityEngine;

namespace Mono
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] public Transform Target; 
        [SerializeField] public Vector3 Offset;
        [SerializeField] public float Smooth; 
        private Vector3 m_velocity;
        [SerializeField] public Vector4 m_range;
        private Transform m_trans;

        private void Awake()
        {
            m_trans = transform;
        }

        private void Update()
        {
            if (Target != null)
            {
                Vector3 pos = Vector3.SmoothDamp(m_trans.position, Target.position + Offset, ref m_velocity,
                    Time.deltaTime * Smooth);

                SetPosition(ref pos);
            }
        }

        private void SetPosition(ref Vector3 pos)
        {
            pos.x = Mathf.Clamp(pos.x, m_range.x, m_range.z);
            pos.y = Mathf.Clamp(pos.y, m_range.y, m_range.w);
            pos.z = -10;
            m_trans.position = pos;
        }
    }
}