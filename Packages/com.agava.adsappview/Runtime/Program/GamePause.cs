using System;
using UnityEngine;

namespace AdsAppView.Program
{
    public class GamePause : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _viewPresenterMonoBehaviour;

        private static GamePause s_instance;

        private IViewPresenter _viewPresenter => _viewPresenterMonoBehaviour as IViewPresenter;

        public static Action GamePaused;
        public static Action GameUnpaused;

        private void OnValidate()
        {
            if (_viewPresenterMonoBehaviour && !(_viewPresenterMonoBehaviour is IViewPresenter))
            {
                Debug.LogError(nameof(_viewPresenterMonoBehaviour) + " needs to implement " + nameof(IViewPresenter));
                _viewPresenterMonoBehaviour = null;
            }
        }

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
