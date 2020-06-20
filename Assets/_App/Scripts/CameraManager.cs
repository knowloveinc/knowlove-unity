using Cinemachine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviourPun
{
    public static CameraManager Instance;

    public CinemachineVirtualCamera[] cameras;

    private void Start()
    {
        Instance = this;
        RPC_SetCamera(2);
    }

    public void SetCamera(int index)
    {
        photonView.RPC("RPC_SetCamera", RpcTarget.All, index);
    }

    [PunRPC]
    public void RPC_SetCamera(int index)
    {
        for(int i = 0; i < cameras.Length; i++)
        {
            cameras[i].enabled = i == index;
        }
    }
}
