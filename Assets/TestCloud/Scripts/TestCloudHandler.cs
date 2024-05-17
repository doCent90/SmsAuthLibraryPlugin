using Agava.Wink;
using Newtonsoft.Json;
using SmsAuthAPI.DTO;
using SmsAuthAPI.Program;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TestCloudHandler : MonoBehaviour
{
    [SerializeField] private Button _savePresf;
    [SerializeField] private Button _loadPrefs;
    [SerializeField] private Button _deletePrefs;
    [SerializeField] private TMP_InputField _input;
    [SerializeField] private Button _checkDevices;

    private IEnumerator Start()
    {
        yield return new WaitWhile(() => WinkSignInHandlerUI.Instance == null);

        _savePresf.onClick.AddListener(OnSavePrefsClicked);
        _loadPrefs.onClick.AddListener(OnLoadPrefsClicked);
        _checkDevices.onClick.AddListener(ShowDevices);
        _deletePrefs.onClick.AddListener(OnDeleteAllClicked);

        WinkSignInHandlerUI.Instance.SignInWindowClosed += OnClosed;
    }

    private void OnClosed()
    {
        Debug.Log("On Closed");
    }

    private void OnSavePrefsClicked()
    {
        Debug.Log("Wink: " + WinkAccessManager.Instance.HasAccess);

        if (WinkAccessManager.Instance.HasAccess == false)
            throw new System.Exception("Wink not authorizated!");

        SmsAuthAPI.Utility.PlayerPrefs.SetString("key", _input.text);
        SmsAuthAPI.Utility.PlayerPrefs.Save();
    }

    private void OnLoadPrefsClicked()
    {
        Debug.Log("Wink: " + WinkAccessManager.Instance.HasAccess);

        if (WinkAccessManager.Instance.HasAccess == false)
            throw new System.Exception("Wink not authorizated!");

        var data = SmsAuthAPI.Utility.PlayerPrefs.GetString("key");

        if (string.IsNullOrEmpty(data))
            Debug.LogError("Load fail: data empty " + data);
        else
            Debug.Log("Loaded: " + data);
    }

    private async void OnDeleteAllClicked()
    {
        Debug.Log("Wink: " + WinkAccessManager.Instance.HasAccess);

        if (WinkAccessManager.Instance.HasAccess == false)
            throw new System.Exception("Wink not authorizated!");

        await SmsAuthAPI.Utility.PlayerPrefs.Load();
        SmsAuthAPI.Utility.PlayerPrefs.DeleteAll();
    }

    private async void ShowDevices()
    {
        Debug.Log("Wink access: " + WinkAccessManager.Instance.HasAccess);

        if (WinkAccessManager.Instance.HasAccess == false)
            throw new System.Exception("Wink not authorizated!");

        Tokens tokens = TokenLifeHelper.GetTokens();
        var response = await SmsAuthApi.GetDevices(tokens.access);

        if(response.statusCode != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error");
        }
        else
        {
            List<string> devices = JsonConvert.DeserializeObject<List<string>>(response.body);

            foreach (var device in devices)
            {
                Debug.Log(device);
            }
        }
    }
}

public class TestData
{
    public string Text;
}
