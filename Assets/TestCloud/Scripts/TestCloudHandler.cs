using Agava.Wink;
using Newtonsoft.Json;
using SmsAuthAPI.DTO;
using SmsAuthAPI.Program;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TestCloudHandler : MonoBehaviour
{
    [SerializeField] private Button _savePresf;
    [SerializeField] private Button _loadPrefs;
    [SerializeField] private TMP_InputField _input;
    [SerializeField] private Button _checkDevices;
    [Header("Server stress test")]
    [SerializeField] private Button _addWriters;
    [SerializeField] private Button _deleteAllWriters;
    [SerializeField] private Button _getOtpCount;
    [SerializeField] private Button _getOtpWrites;
    [SerializeField] private ulong _writesCount;
    [SerializeField] private string _number;
    [SerializeField] private string _targetOtp;

    private IEnumerator Start()
    {
        yield return new WaitWhile(() => WinkSignInHandlerUI.Instance == null);

        _savePresf.onClick.AddListener(OnSavePrefsClicked);
        _loadPrefs.onClick.AddListener(OnLoadPrefsClicked);
        _checkDevices.onClick.AddListener(ShowDevices);

        _addWriters.onClick.AddListener(CreateWriters);
        _deleteAllWriters.onClick.AddListener(DeleteAllWrites);
        _getOtpCount.onClick.AddListener(GetOtpCount);
        _getOtpWrites.onClick.AddListener(GetOtpWrites);

        WinkSignInHandlerUI.Instance.SignInWindowClosed += OnClosed;
    }

    private async void CreateWriters()
    {
        Response response = await SmsAuthApi.Write(_number, _writesCount);

        if (response.statusCode != UnityWebRequest.Result.Success)
            Debug.LogError("Create Error : " + response.statusCode);
        else
            Debug.Log("Create Done : " + response.statusCode);
    }

    private async void DeleteAllWrites()
    {
        var result = await SmsAuthApi.ClearOtpTable();

        Debug.Log("Otp Table Cleared: " + result.statusCode);
    }

    private async void GetOtpWrites()
    {
        var result = await SmsAuthApi.GetOtpsWrites(_targetOtp);

        Debug.Log("Phone by otp: " + result.body);
    }

    private async void GetOtpCount()
    {
        var result = await SmsAuthApi.GetOtpsCount();

        Debug.Log("Otps Count: " + result.body);
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
            Debug.Log($"<color=red>Load fail</color>: data empty {data}");
        else
            Debug.Log("Loaded: " + data);
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
