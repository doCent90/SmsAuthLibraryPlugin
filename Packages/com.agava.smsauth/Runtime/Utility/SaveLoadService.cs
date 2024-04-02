using Newtonsoft.Json;
using UnityEngine;

namespace Utility
{
    internal static class SaveLoadService
    {
        internal static void Save<T>(T obj, string saveName) where T : class
        {
            string data = JsonConvert.SerializeObject(obj);
            PlayerPrefs.SetString(saveName, data);
            Debug.Log($"<color = green>Saved</color>");
        }

        internal static T Load<T>(string saveName) where T : class
        {
            if (PlayerPrefs.HasKey(saveName) == false)
            {
                Debug.Log($"<color = green>Load Fail</color>");
                return null;
            }

            var loadData = PlayerPrefs.GetString(saveName);
            T data = JsonConvert.DeserializeObject<T>(loadData);

            Debug.Log($"<color = green>Loaded</color>");
            return data;
        }

        internal static void Delete(string saveName)
        {
            PlayerPrefs.DeleteKey(saveName);
            Debug.Log($"<color = green>Delete Saves</color>");
        }
    }
}
