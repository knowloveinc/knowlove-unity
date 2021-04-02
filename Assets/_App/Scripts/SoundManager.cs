using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Knowlove
{
    [RequireComponent(typeof(PhotonView))]
    public class SoundManager : MonoBehaviourPun
    {
        public static SoundManager Instance;

        public AudioSource source;

        public AudioClip[] clips;

        private void Start()
        {
            Instance = this;
        }

        public void PlaySound(string audioClipName, Player player = null)
        {
            if (player == null)
            {
                photonView.RPC("RPC_PlaySound", RpcTarget.All, audioClipName);
            }
            else
            {
                photonView.RPC("RPC_PlaySound", player, audioClipName);
            }
        }

        [PunRPC]
        void RPC_PlaySound(string audioClipName)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].name.ToLower() == audioClipName.ToLower())
                {
                    source.PlayOneShot(clips[i]);
                }
            }
        }
    }
}

