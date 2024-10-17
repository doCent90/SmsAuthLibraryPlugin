using System.Collections;
using AdsAppView.DTO;
using AdsAppView.Utility;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace AdsAppView.Program
{
    public class VideoViewPresenter : BaseViewPresenter
    {
        [SerializeField] private VideoPlayer _player;

        private RawImage _videoPlayerImage;

        public override void Show(PopupData popupData)
        {
            _videoPlayerImage = _player.GetComponent<RawImage>();
            link = popupData.link;
            lastSpriteName = popupData.name;
            _player.url = popupData.cacheFilePath;
            EnableCanvasGroup();
        }

        protected override IEnumerator Enabling(CanvasGroup canvas)
        {
            _videoPlayerImage.enabled = false;

            float enablingTime = ViewPresenterConfigs.EnablingTime;
            WaitForSecondsRealtime waitForFadeIn = new WaitForSecondsRealtime(enablingTime);

            StartCoroutine(FadeIn.FadeInGraphic(background, enablingTime));
            yield return new WaitForSecondsRealtime(Diff);

            StartCoroutine(FadeIn.FadeInGraphic(_videoPlayerImage, enablingTime));
            yield return new WaitForSecondsRealtime(enablingTime);

            _player.Play();
            _player.loopPointReached += OnLoopPointReached;

            void OnLoopPointReached(VideoPlayer _)
            {
                closeButton.gameObject.SetActive(true);
            }

            linkButton.gameObject.SetActive(true);
            StartCoroutine(FadeIn.FadeInGraphic(linkButtonImage, enablingTime));
            yield return waitForFadeIn;
        }
    }
}
