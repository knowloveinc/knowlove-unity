using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GameBrewStudios
{
    public class CardDeck : MonoBehaviour
    {
        [SerializeField]
        GameObject card;

        [SerializeField]
        string deckName;


        new Transform camera;


        private List<Card> spawnedCards = new List<Card>();

        private void Start()
        {
            camera = Camera.main.transform;
        }

        
        public void DrawCard(System.Action OnAnimationFinished = null)
        {
            //string cardText = GameManager.Instance.GetRandomCard(deckName);
            Debug.Log("Spawning card...");
            GameObject newCardObj = Instantiate(card, transform);
            newCardObj.SetActive(true);
            Debug.Log("Animating card...");
            SimpleAnimateCard(newCardObj.transform, OnAnimationFinished);
            //Card newCard = newCardObj.GetComponent<Card>();
            //newCard.Init(cardText, this);

            //AnimateDrawCard(newCard.transform);

            //return newCard;
        }

        void SimpleAnimateCard(Transform cardObj, System.Action OnAnimationFinished = null)
        {
            cardObj.DOMove(new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z), 0.25f).OnComplete(() => 
            {
                Destroy(cardObj.gameObject);
                OnAnimationFinished?.Invoke();
            });
        }

        void AnimateDrawCard(Transform cardTransform)
        {
            if (camera == null) camera = Camera.main.transform;

            List<Vector3> path = new List<Vector3>()
            {
                cardTransform.position,
                new Vector3(cardTransform.position.x, cardTransform.position.y + 0.05f, cardTransform.position.z - 0.1f),
                Camera.main.transform.position
            };

            cardTransform.DOPath(path.ToArray(), 1f, PathType.Linear, PathMode.Full3D, 10)
                .OnWaypointChange((next) =>
                {
                    if (next >= 1)
                    {
                        //cardTransform.eulerAngles = Vector3.Lerp(cardTransform.eulerAngles, new Vector3(42f, 0f, 0f), 0.5f);

                    
                    }
                })
                .OnUpdate(() => 
                {
                    cardTransform.rotation = Quaternion.Lerp(cardTransform.rotation, Quaternion.LookRotation(cardTransform.position - camera.transform.position), 6f * Time.deltaTime);
                });
        }
    }
}