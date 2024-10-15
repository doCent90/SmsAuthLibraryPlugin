using System;
using System.Collections;
using AdsAppView.DTO;
using AdsAppView.Utility;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace AdsAppView.Program
{
    public class VideoViewPresenter : MonoBehaviour, IViewPresenter
    {
        private const float Diff = 0.5f;

        [SerializeField] private CanvasGroup _windowCanvasGrp;
        [SerializeField] private Button _linkButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        [SerializeField] private Image _background;
        [SerializeField] private VideoPlayer _player;

        private string _link;
        private string _lastSpriteName;
        private int _count = 0;

        public bool Enable { get; private set; } = false;

        public event Action Enabled;
        public event Action Disabled;

        private void Awake()
        {
            DisableCanvasGroup(_windowCanvasGrp);
        }

        private void OnEnable()
        {
            _linkButton.onClick.AddListener(OnLinkClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnDisable()
        {
            _linkButton.onClick.RemoveListener(OnLinkClicked);
            _closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        public void Show(PopupData sprite)
        {
            _count++;
            _link = sprite.link;
            _lastSpriteName = sprite.name;

            //_player.url = savePath;

            _player.loopPointReached += OnLoopReached;
            _player.Play();
        }

        private void OnLinkClicked()
        {
#if UNITY_EDITOR
            Debug.LogFormat($"#ViewPresenter# Open link {_link}");
#endif

            if (string.IsNullOrEmpty(_link))
                return;

            AnalyticsService.SendPopupRedirectClick(_lastSpriteName, _count);
            Application.OpenURL(_link);
        }

        private void OnLoopReached(VideoPlayer _)
        {
            _player.Stop();
            _closeButton.gameObject.SetActive(true);
        }

        private void OnCloseClicked()
        {
            AnalyticsService.SendPopupClosed();
            DisableCanvasGroup(_windowCanvasGrp);
        }

        private void DisableCanvasGroup(CanvasGroup canvas)
        {
            canvas.alpha = 0;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
            Enable = false;
            Disabled?.Invoke();
        }
    }
}
