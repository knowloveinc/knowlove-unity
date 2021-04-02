using UnityEngine;
using UnityEngine.UI;

namespace Knowlove
{
    [RequireComponent(typeof(AspectRatioFitter))]
    public class AspectRatioUpdater : MonoBehaviour
    {
        AspectRatioFitter fitter;
        Image image;
        private void Awake()
        {
            fitter = GetComponent<AspectRatioFitter>();
            image = GetComponent<Image>();
        }

        void Update()
        {
            fitter.aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
        }
    }
}

