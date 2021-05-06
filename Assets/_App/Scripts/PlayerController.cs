using Newtonsoft.Json;
using Photon.Pun;

namespace Knowlove
{
    public class PlayerController : MonoBehaviourPun, IPunObservable
    {
        public string owner;

        public string playerProps;

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                owner = photonView.Owner.NickName;

                stream.SendNext(owner);

                playerProps = JsonConvert.SerializeObject(photonView.Owner.CustomProperties);
                stream.SendNext(playerProps);
            }
            else
            {
                owner = (string)stream.ReceiveNext();

                playerProps = (string)stream.ReceiveNext();
            }
        }

    }
}
