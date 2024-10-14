using System.Collections;
using System.Threading.Tasks;
using AdsAppView.DTO;
using AdsAppView.Utility;
using UnityEngine;
using UnityEngine.Networking;

public class ViewPresenterConfigs : MonoBehaviour
{
    private const string ClosingDelayConfig = "closing-delay";
    private const string EnablingTimeConfig = "enabling-time";

    private AdsAppAPI _api;

    public static float ClosingDelay { get; private set; } = 2;
    public static float EnablingTime { get; private set; } = 2;

    public IEnumerator Initialize(AdsAppAPI api)
    {
        _api = api;

        var waitWeb = new WaitUntil(() => Application.internetReachability == NetworkReachability.NotReachable);
        var waitInit = new WaitUntil(() => _api.Initialized);

        if (Application.internetReachability == NetworkReachability.NotReachable)
            yield return waitWeb;

        yield return waitInit;
        yield return new WaitForSecondsRealtime(3f);

        SetConfigs();
    }

    private async void SetConfigs()
    {
        await SetEnablingTimeConfig();
        await SetClosingDelayConfig();
    }

    private async Task SetClosingDelayConfig()
    {
        Response cachingResponse = await AdsAppAPI.Instance.GetRemoteConfig(ClosingDelayConfig);

        if (cachingResponse.statusCode == UnityWebRequest.Result.Success)
        {
            string body = cachingResponse.body;

            if (float.TryParse(body, out float closingDelay))
            {
                ClosingDelay = closingDelay;
#if UNITY_EDITOR
                Debug.Log("#PopupManager# Closing delay set to: " + ClosingDelay);
#endif
            }
        }
        else
        {
            Debug.LogError("#PopupManager# Fail to Set Closing Delay Config whith error: " + cachingResponse.statusCode);
        }
    }

    private async Task SetEnablingTimeConfig()
    {
        Response cachingResponse = await AdsAppAPI.Instance.GetRemoteConfig(EnablingTimeConfig);

        if (cachingResponse.statusCode == UnityWebRequest.Result.Success)
        {
            string body = cachingResponse.body;

            if (float.TryParse(body, out float enablingTime))
            {
                EnablingTime = enablingTime;
#if UNITY_EDITOR
                Debug.Log("#PopupManager# Enabling time set to: " + EnablingTime);
#endif
            }
        }
        else
        {
            Debug.LogError("#PopupManager# Fail to Set Enabling Time Config whith error: " + cachingResponse.statusCode);
        }
    }
}
