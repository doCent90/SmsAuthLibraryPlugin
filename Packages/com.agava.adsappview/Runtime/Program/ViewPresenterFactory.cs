using System.Collections.Generic;
using UnityEngine;

namespace AdsAppView.Program
{
    public class ViewPresenterFactory : MonoBehaviour
    {
        [SerializeField] private ImageViewPresenter _imageViewPresenter;
        [SerializeField] private VideoViewPresenter _videoViewPresenter;
        [SerializeField] private WebViewPresenter _webViewPresenter;

        private Dictionary<string, GameObject> _mapping;

        public IViewPresenter InstantiateViewPresenter()
        {
            _mapping ??= new Dictionary<string, GameObject>()
            {
                { "image", _imageViewPresenter.gameObject},
                { "video", _videoViewPresenter.gameObject},
                { "web", _webViewPresenter.gameObject},
            };

            GameObject viewPresenter;

            string type = ViewPresenterConfigs.ViewPresenterType;

            if (_mapping.TryGetValue(type, out viewPresenter))
            {
                viewPresenter = Instantiate(viewPresenter, transform);
            }
            else
            {
                viewPresenter = Instantiate(_webViewPresenter.gameObject, transform);
            }

            return viewPresenter.GetComponent<IViewPresenter>();
        }
    }
}
