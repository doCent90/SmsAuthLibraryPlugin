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
    [SerializeField] private TMP_InputField _input;
    [SerializeField] private Button _checkDevices;

    private void Start()
    {
        _save.onClick.AddListener(OnSaveClicked);
        _load.onClick.AddListener(OnLoadClicked);
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

    private async void OnLoadClicked()
    {
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
