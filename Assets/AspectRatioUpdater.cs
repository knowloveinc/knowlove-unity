using UnityEngine;
using UnityEngine.UI;

namespace Knowlove
{
    [RequireComponent(typeof(AspectRatioFitter))]
    public class AspectRatioUpdater : MonoBehaviour
    {
        private AspectRatioFitter _fitter;
        private Image _image;
        private void Awake()
        {
            _fitter = GetComponent<AspectRatioFitter>();
            _image = GetComponent<Image>();
        }

        void Update()
        {
            _fitter.aspectRatio = _image.sprite.rect.width / _image.sprite.rect.height;
        }
    }
}

