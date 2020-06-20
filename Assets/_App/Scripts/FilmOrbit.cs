using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FilmOrbit : MonoBehaviour
{
    public float speed = 10f;

    private Cinemachine.CinemachineVirtualCamera m_VirtualCam;

    void Start()
    {
        if (GetComponent<Cinemachine.CinemachineVirtualCamera>())
            m_VirtualCam = GetComponent<Cinemachine.CinemachineVirtualCamera>();
    }

    void Update()
    {
        if (m_VirtualCam.GetCinemachineComponent<CinemachineOrbitalTransposer>())
            m_VirtualCam.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.Value += Time.deltaTime * speed;
    }
}