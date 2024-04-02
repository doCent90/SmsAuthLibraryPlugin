using Agava.Wink;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;

public class TestCloudHandler : MonoBehaviour
{
    [SerializeField] private Button _save;
    [SerializeField] private Button _load;
    [SerializeField] private TMP_InputField _input;

    private void Start()
    {
        _save.onClick.AddListener(OnSaveClicked);
        _load.onClick.AddListener(OnLoadClicked);
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
}

public class TestData
{
    public string Text;
}
