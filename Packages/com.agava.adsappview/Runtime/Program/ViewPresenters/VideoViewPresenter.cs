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
        [SerializeField] private Image _background;
        [SerializeField] private VideoPlayer _player;

        private Image _linkButtonImage;
        private RawImage _videoPlayerImage;

        private string _link;
        private string _lastSpriteName;
        private int _count = 0;

        private Coroutine _enablingCoroutine;

        public bool Enable { get; private set; } = false;

        public event Action Enabled;
        public event Action Disabled;

        private void Awake()
        {
            DisableCanvasGroup(_windowCanvasGrp);
            _linkButtonImage = _linkButton.GetComponent<Image>();
            _videoPlayerImage = _player.GetComponent<RawImage>();
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

        public void Show(PopupData popupData)
        {
            _count++;
            _link = popupData.link;
            _lastSpriteName = popupData.name;
            _player.url = popupData.cacheFilePath;
            Stop(_enablingCoroutine);
            _enablingCoroutine = StartCoroutine(EnableCanvasGroup(_windowCanvasGrp));
        }

        private IEnumerator EnableCanvasGroup(CanvasGroup canvas)
        {
            _closeButton.gameObject.SetActive(false);
            _linkButton.gameObject.SetActive(false);

            _background.enabled = false;
            _videoPlayerImage.enabled = false;
            _linkButtonImage.enabled = false;

            canvas.interactable = true;
            canvas.blocksRaycasts = true;
            canvas.alpha = 1;
            Enable = true;
            Enabled?.Invoke();

            float enablingTime = ViewPresenterConfigs.EnablingTime;
            WaitForSecondsRealtime waitForFadeIn = new WaitForSecondsRealtime(enablingTime);

            StartCoroutine(FadeIn.FadeInGraphic(_background, enablingTime));
            yield return new WaitForSecondsRealtime(Diff);

            StartCoroutine(FadeIn.FadeInGraphic(_videoPlayerImage, enablingTime));
            yield return new WaitForSecondsRealtime(enablingTime);

            _player.Play();
            _player.loopPointReached += OnLoopPointReached;

            void OnLoopPointReached(VideoPlayer _)
            {
                _closeButton.gameObject.SetActive(true);
            }

            _linkButton.gameObject.SetActive(true);
            StartCoroutine(FadeIn.FadeInGraphic(_linkButtonImage, enablingTime));
            yield return waitForFadeIn;
        }

        private void DisableCanvasGroup(CanvasGroup canvas)
        {
            canvas.alpha = 0;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
            Enable = false;
            Disabled?.Invoke();
            Stop(_enablingCoroutine);
        }

        private void OnCloseClicked()
        {
            AnalyticsService.SendPopupClosed();
            DisableCanvasGroup(_windowCanvasGrp);
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

        private void Stop(Coroutine coroutine)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
    }
}
