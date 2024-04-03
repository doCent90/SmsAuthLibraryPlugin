using Agava.Wink;
using Newtonsoft.Json;
using SmsAuthAPI.DTO;
using SmsAuthAPI.Program;
using SmsAuthAPI.Utility;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestCloudHandler : MonoBehaviour
{
    [SerializeField] private Button _save;
    [SerializeField] private Button _load;
    [SerializeField] private Button _savePresf;
    [SerializeField] private Button _loadPrefs;
    [SerializeField] private TMP_InputField _input;
    [SerializeField] private Button _checkDevices;

    private void Start()
    {
        _save.onClick.AddListener(OnSaveClicked);
        _savePresf.onClick.AddListener(OnSavePrefsClicked);
        _load.onClick.AddListener(OnLoadClicked);
        _loadPrefs.onClick.AddListener(OnLoadPrefsClicked);
        _checkDevices.onClick.AddListener(ShowDevices);
    }

    private void OnSaveClicked()
    {
        Debug.Log("Wink: " + WinkAccessManager.Instance.HasAccess);

        if (WinkAccessManager.Instance.HasAccess == false)
            throw new System.Exception("Wink not authorizated!");

        TestData data = new()
        {
            Text = _input.text,
        };

        SaveLoadCloudDataService.SaveData(data);
    }

    private void OnSavePrefsClicked()
    {
        Debug.Log("Wink: " + WinkAccessManager.Instance.HasAccess);

        if (WinkAccessManager.Instance.HasAccess == false)
            throw new System.Exception("Wink not authorizated!");

        SmsAuthAPI.Utility.PlayerPrefs.SetString("key", _input.text);
        SmsAuthAPI.Utility.PlayerPrefs.Save();
    }

    private async void OnLoadPrefsClicked()
    {
        Debug.Log("Wink: " + WinkAccessManager.Instance.HasAccess);

        if (WinkAccessManager.Instance.HasAccess == false)
            throw new System.Exception("Wink not authorizated!");

        await SmsAuthAPI.Utility.PlayerPrefs.Load();
        var data = SmsAuthAPI.Utility.PlayerPrefs.GetString("key");

        if (string.IsNullOrEmpty(data))
            Debug.LogError("Load fail");
        else
            Debug.Log("Loaded: " + data);
    }

    private async void OnLoadClicked()
    {
        Debug.Log("Wink: " + WinkAccessManager.Instance.HasAccess);

        if (WinkAccessManager.Instance.HasAccess == false)
            throw new System.Exception("Wink not authorizated!");

        var data = await SaveLoadCloudDataService.LoadData<TestData>();

        if (data != null)
        {
            Debug.Log("Loaded: " + data.Text);
        }
    }

    private async void ShowDevices()
    {
        Debug.Log("Wink access: " + WinkAccessManager.Instance.HasAccess);

        if (WinkAccessManager.Instance.HasAccess == false)
            throw new System.Exception("Wink not authorizated!");

        Tokens tokens = TokenLifeHelper.GetTokens();
        var response = await SmsAuthApi.GetDevices(tokens.access);

        if(response.statusCode != (uint)YbdStatusCode.Success)
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
