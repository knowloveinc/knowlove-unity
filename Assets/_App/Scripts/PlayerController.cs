using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    public string owner;

    public string playerProps;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            owner = photonView.Owner.NickName;
            Debug.Log("owner set to " + owner, this.gameObject);

            
            stream.SendNext(owner);

            playerProps = JsonConvert.SerializeObject(photonView.Owner.CustomProperties);
            stream.SendNext(playerProps);
        }
        else
        {
            owner = (string)stream.ReceiveNext();
            Debug.Log("owner set to " + owner, this.gameObject);

            playerProps = (string)stream.ReceiveNext();
        }
    }

}
