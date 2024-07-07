using Agava.Wink;
using Newtonsoft.Json;
using SmsAuthAPI.DTO;
using SmsAuthAPI.Program;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
    [Header("Server stress test 2")]
    [SerializeField] private Button _addSaves;
    [SerializeField] private Button _addMassiveSaves;
    [SerializeField] private Button _getSave;
    [SerializeField] private Button _deleteAllSaves;
    [SerializeField] private string _phoneNumber;
    [SerializeField] private string _savesBodyString;
    [SerializeField] private int _savesSize;
    [SerializeField] private string _clearPassword;
    [SerializeField] private int _countMassiveSaves;
    [Header("Server http client")]
    [SerializeField] private string _path;
    [SerializeField] private Button _send;
    [Header("Lang buttons")]
    [SerializeField] private Button _ruButton;
    [SerializeField] private Button _enButton;

    private HttpClient _client;

    private IEnumerator Start()
    {
        yield return new WaitWhile(() => WinkSignInHandlerUI.Instance == null);

        _savePresf.onClick.AddListener(OnSavePrefsClicked);
        _loadPrefs.onClick.AddListener(OnLoadPrefsClicked);
        _checkDevices.onClick.AddListener(ShowDevices);

        _addSaves.onClick.AddListener(OnSavesClicked);
        _addMassiveSaves.onClick.AddListener(OnMassiveSavesClicked);
        _getSave.onClick.AddListener(OnGetCurrentSavesClicked);

        _send.onClick.AddListener(OnSendClicked);
        _ruButton.onClick.AddListener(OnRuClicked);
        _enButton.onClick.AddListener(OnEnClicked);

        _client = new HttpClient();
        WinkSignInHandlerUI.Instance.SignInWindowClosed += OnClosed;
    }

    private void OnEnClicked() => WinkLocalization.Instance.SetCurrentLang(SystemLanguage.English);

    private void OnRuClicked() => WinkLocalization.Instance.SetCurrentLang(SystemLanguage.Russian);

    private async void OnSendClicked()
    {
        var response = await _client.PostAsync(_path, new StringContent(string.Empty));
        response.EnsureSuccessStatusCode();
    }

    private async void OnMassiveSavesClicked()
    {
        //Debug.Log("Wink: " + WinkAccessManager.Instance.HasAccess);

        //if (WinkAccessManager.Instance.HasAccess == false)
        //    throw new System.Exception("Wink not authorizated!");

        //for (int i = 0; i < _countMassiveSaves; i++)
        //{
        //    StringBuilder sb = new();

        //    for (int x = 0; x < _savesSize; x++)
        //        sb.Append(_savesBodyString);

        //    TestData testData = new()
        //    {
        //        Text = sb.ToString()
        //    };

        //    var json = JsonConvert.SerializeObject(testData);

        //    await SmsAuthApi.WriteSaveClouds(_phoneNumber + i, json);
        //}

        //Debug.LogError("Massive Saves DONE");
    }

    private async void OnSavesClicked()
    {
        //Debug.Log("Wink: " + WinkAccessManager.Instance.HasAccess);

        //if (WinkAccessManager.Instance.HasAccess == false)
        //    throw new System.Exception("Wink not authorizated!");

        //StringBuilder sb = new();

        //for (int i = 0; i < _savesSize; i++)
        //    sb.Append(_savesBodyString);

        //TestData testData = new()
        //{
        //    Text = sb.ToString()
        //};

        //var json = JsonConvert.SerializeObject(testData);

        //await SmsAuthApi.WriteSaveClouds(_phoneNumber, json);
    }

    private async void OnGetCurrentSavesClicked()
    {
        //Debug.Log("Wink: " + WinkAccessManager.Instance.HasAccess);

        //if (WinkAccessManager.Instance.HasAccess == false)
        //    throw new System.Exception("Wink not authorizated!");

        //var data = await SmsAuthApi.GetSaveCloud(_phoneNumber);

        //if (string.IsNullOrEmpty(data.body))
        //    Debug.Log($"<color=red>Load fail</color>: data empty {data.body}");
        //else
        //    Debug.Log("Loaded: " + data.body);
    }

    //private async void OnClearAllSavesClicked()
    //{
    //    await SmsAuthApi.ClearAllSaveCloud(_clearPassword);
    //}

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

        if (WinkAccessManager.Instance.Authenficated == false)
            throw new System.Exception("Wink not authenticated!");

        Tokens tokens = TokenLifeHelper.GetTokens();
        var response = await SmsAuthApi.GetDevices(tokens.access, Application.identifier);

        if (response.statusCode != UnityWebRequest.Result.Success)
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
