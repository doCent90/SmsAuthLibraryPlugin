using AdsAppView.Program;
using UnityEngine;

public class ViewPresenterFactory : MonoBehaviour
{
    [SerializeField] private ImageViewPresenter _imageViewPresenter;
    [SerializeField] private VideoViewPresenter _videoViewPresenter;
    [SerializeField] private WebViewPresenter _webViewPresenter;

    public IViewPresenter InstantiateViewPresenter()
    {
        var viewPresenter = Instantiate(_imageViewPresenter, transform);
        viewPresenter.gameObject.SetActive(true);
        return viewPresenter;
    }
}
