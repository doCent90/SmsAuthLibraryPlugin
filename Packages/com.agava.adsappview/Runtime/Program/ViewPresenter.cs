using System;
using System.Collections;
using System.IO;
using System.Net;
using AdsAppView.DTO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace AdsAppView.Program
{
    public class ViewPresenter : MonoBehaviour
    {
        private const float Diff = 0.5f;

        [SerializeField] private CanvasGroup _windowCanvasGrp;
        [SerializeField] private Button _linkButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        [SerializeField] private Image _popupImage;
        [SerializeField] private Image _background;

        private Image _linkButtonImage;

        private float _closingDelay = 0;
        private float _enablingTime = 10;
        private string _link;

        private Coroutine _enablingCoroutine;

        public bool Enable { get; private set; } = false;

        public Action Enabled;
        public Action Disabled;

        [SerializeField] private VideoPlayer _player;

        public IEnumerator ViewGifCoroutine()
        {
            string ftpUrl = "ftp://ftp-p.ctcmedia.ru/mediartk/ubarf.mp4";

            if (Uri.TryCreate(ftpUrl, UriKind.Absolute, out Uri uri) == false)
                throw new NullReferenceException("Cant create uri: " + ftpUrl);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);

            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = true;

            request.Credentials = new NetworkCredential("Statistics_rw", "WQpAax1Q");
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            Debug.Log("#View# name to load: " + ftpUrl);

            WebResponse responce = request.GetResponse();

            yield return new WaitForSecondsRealtime(2f);

            string savePath = Application.persistentDataPath;

            if (Directory.Exists(savePath) == false)
            {
                Directory.CreateDirectory(savePath);
                Debug.Log($"#View# Created folder: " + savePath);
            }
            else
            {
                Debug.Log($"#View# Folder exist: " + savePath);
            }

            savePath = Application.persistentDataPath + "/ubarf.mp4";

            Debug.Log("#View# Start stream: " + ftpUrl);
            using (Stream reader = responce.GetResponseStream())
            {
                using (FileStream fileStream = new(savePath, FileMode.Create))
                {
                    int bytesRead = 0;
                    byte[] buffer = new byte[2048];

                    while (true)
                    {
                        bytesRead = reader.Read(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                            break;

                        fileStream.Write(buffer, 0, bytesRead);
                    }

                    fileStream.Close();
                }
            }

            _player.url = savePath;
            _player.Play();
            Debug.Log("#View# Stream Finish: " + ftpUrl);
        }

        private void Awake()
        {
            StartCoroutine(ViewGifCoroutine());

            DisableCanvasGroup(_windowCanvasGrp);
            _linkButtonImage = _linkButton.GetComponent<Image>();
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

        public void Initialize(float enablingTime, float closingDelay)
        {
            _enablingTime = enablingTime;
            _closingDelay = closingDelay;
        }

        public void Show(SpriteData sprite)
        {
            _popupImage.sprite = sprite.sprite;
            _aspectRatioFitter.aspectRatio = sprite.aspectRatio;
            _link = sprite.link;

            Stop(_enablingCoroutine);
            _enablingCoroutine = StartCoroutine(EnableCanvasGroup(_windowCanvasGrp));
        }

        private void OnLinkClicked()
        {
#if UNITY_EDITOR
            Debug.LogFormat($"Open link {_link}");
#endif

            if (string.IsNullOrEmpty(_link))
                return;

            Application.OpenURL(_link);
        }

        private void OnCloseClicked() => DisableCanvasGroup(_windowCanvasGrp);

        private IEnumerator EnableCanvasGroup(CanvasGroup canvas)
        {
            _closeButton.gameObject.SetActive(false);
            _linkButton.gameObject.SetActive(false);

            _popupImage.enabled = false;
            _background.enabled = false;
            _linkButtonImage.enabled = false;

            canvas.interactable = true;
            canvas.blocksRaycasts = true;
            canvas.alpha = 1;
            Enable = true;
            Enabled?.Invoke();

            WaitForSecondsRealtime waitForFadeIn = new WaitForSecondsRealtime(_enablingTime);

            StartCoroutine(FadeInImage(_background, _enablingTime));
            yield return new WaitForSecondsRealtime(Diff);

            StartCoroutine(FadeInImage(_popupImage, _enablingTime));
            yield return waitForFadeIn;

            _linkButton.gameObject.SetActive(true);

            StartCoroutine(FadeInImage(_linkButtonImage, _enablingTime));
            yield return waitForFadeIn;

            yield return new WaitForSecondsRealtime(_closingDelay);
            _closeButton.gameObject.SetActive(true);
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

        private IEnumerator FadeInImage(Image image, float time)
        {
            image.enabled = true;

            image.gameObject.SetActive(true);
            Color color = image.color;

            if (time > 0)
            {
                color.a = 0;
                float elapsedTime = 0;

                while (elapsedTime < _enablingTime)
                {
                    color.a = Mathf.Lerp(0, 1, elapsedTime / _enablingTime);
                    image.color = color;
                    elapsedTime += Time.unscaledDeltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }

            color.a = 1;
            image.color = color;
        }

        private void Stop(Coroutine coroutine)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
    }
}
