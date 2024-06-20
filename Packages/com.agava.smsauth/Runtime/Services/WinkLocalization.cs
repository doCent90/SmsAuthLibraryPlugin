using Lean.Localization;
using UnityEngine;

namespace Agava.Wink
{
    public class WinkLocalization : MonoBehaviour
    {
        [SerializeField] private LeanLocalization _leanLocalization;

        public static WinkLocalization Instance;

        private void Awake()
        {
            DontDestroyOnLoad(this);

            Instance = this;

            ChangeLang(Application.systemLanguage);
        }

        public void ChangeLang(SystemLanguage lang)
        {
            switch (lang)
            {
                case SystemLanguage.Russian:
                    SetRuLang();
                    break;
                default:
                    SetEnLang();
                    break;
            }
        }

        private void SetRuLang() => _leanLocalization.CurrentLanguage = "ru";

        private void SetEnLang() => _leanLocalization.CurrentLanguage = "en";
    }
}