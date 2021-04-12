using UnityEngine;
using Cinemachine;

namespace Knowlove
{
    public class FilmOrbit : MonoBehaviour
    {
        public float speed = 10f;

        private CinemachineVirtualCamera m_VirtualCam;

        void Start()
        {
            if (GetComponent<CinemachineVirtualCamera>())
                m_VirtualCam = GetComponent<CinemachineVirtualCamera>();
        }

        void Update()
        {
            if (m_VirtualCam.GetCinemachineComponent<CinemachineOrbitalTransposer>())
                m_VirtualCam.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.Value += Time.deltaTime * speed;
        }
    }
}