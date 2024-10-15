using System;
using UnityEngine;

namespace AdsAppView.Program
{
    public class GamePause : MonoBehaviour
    {
        [SerializeField] private PopupManager _popupManager;

        private static GamePause s_instance;
        private IViewPresenter _viewPresenter => _popupManager.ViewPresenter;

        public static Action GamePaused;
        public static Action GameUnpaused;

        private void Awake()
        {
            if (s_instance == null)
            {
                s_instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            s_instance._viewPresenter.Enabled += OnAdsEnabled;
            s_instance._viewPresenter.Disabled += OnAdsDisabled;
        }

        private void OnDisable()
        {
            s_instance._viewPresenter.Enabled -= OnAdsEnabled;
            s_instance._viewPresenter.Disabled -= OnAdsDisabled;
        }

        private void OnAdsEnabled()
        {
            GamePaused?.Invoke();
        }

        private void OnAdsDisabled()
        {
            GameUnpaused?.Invoke();
        }
    }
}
