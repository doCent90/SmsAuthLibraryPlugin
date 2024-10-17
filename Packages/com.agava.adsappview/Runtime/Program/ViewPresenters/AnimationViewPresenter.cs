using System.Collections;
using AdsAppView.DTO;
using AdsAppView.Utility;
using Spine.Unity;
using UnityEngine;

namespace AdsAppView.Program
{
    public class AnimationViewPresenter : BaseViewPresenter
    {
        [SerializeField] private SkeletonGraphic _skeletonGraphic;

        public override void Show(PopupData popupData)
        {
            link = popupData.link;
            lastSpriteName = popupData.name;

            EnableCanvasGroup();
        }

        protected override IEnumerator Enabling(CanvasGroup canvas)
        {
            _skeletonGraphic.enabled = false;

            float enablingTime = ViewPresenterConfigs.EnablingTime;
            float closingDelay = ViewPresenterConfigs.ClosingDelay;

            WaitForSecondsRealtime waitForFadeIn = new WaitForSecondsRealtime(enablingTime);

            StartCoroutine(FadeIn.FadeInGraphic(background, enablingTime));
            yield return new WaitForSecondsRealtime(Diff);

            StartCoroutine(FadeIn.FadeInGraphic(_skeletonGraphic, enablingTime));
            yield return waitForFadeIn;

            linkButton.gameObject.SetActive(true);

            StartCoroutine(FadeIn.FadeInGraphic(linkButtonImage, enablingTime));
            yield return waitForFadeIn;

            yield return new WaitForSecondsRealtime(closingDelay);
            closeButton.gameObject.SetActive(true);
        }
    }
}
