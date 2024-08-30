using System;
using System.Collections;
using SmsAuthAPI.Program;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    internal class TextTimer : MonoBehaviour
    {
        private const string RemoteName = "sms-delay-seconds";
        private const int DefaultTime = 300;

        [SerializeField] private TextPlaceholder _timePlaceholder;

        private int _seconds = 0;
        private Coroutine _coroutine;

        public event Action TimerExpired;

        private IEnumerator Start()
        {
            if (_seconds <= 0)
                _seconds = DefaultTime;

            yield return new WaitUntil(() => SmsAuthApi.Initialized);
            SetRemoteConfig();
        }

        internal void Enable()
        {
            _timePlaceholder.gameObject.SetActive(true);
            _coroutine = StartCoroutine(Ticking());

            IEnumerator Ticking()
            {
                var tick = new WaitForSecondsRealtime(1);

                while (_seconds > 0)
                {
                    _seconds--;
                    _timePlaceholder.ReplaceValue(_seconds.ToString());

                    yield return tick;
                }

                if (_seconds <= 0)
                    TimerExpired?.Invoke();
            }
        }

        internal void Disable()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
                _timePlaceholder.gameObject.SetActive(false);
            }
        }

        private async void SetRemoteConfig()
        {
            var response = await SmsAuthApi.GetRemoteConfig(RemoteName);

            if (response.statusCode == UnityWebRequest.Result.Success)
            {
                if (string.IsNullOrEmpty(response.body))
                {
                    _seconds = DefaultTime;
                    Debug.LogError($"Fail to recieve remote config '{RemoteName}': value is NULL");
                }
                else
                {
                    bool success = int.TryParse(response.body, out int time);

                    if (success)
                        _seconds = time;
                    else
                        _seconds = DefaultTime;
                }
            }
            else
            {
                Debug.LogError($"Fail to recieve remote config '{RemoteName}': BAD REQUEST");
            }
        }
    }
}
