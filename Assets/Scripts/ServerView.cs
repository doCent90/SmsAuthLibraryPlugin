using UnityEngine;
using UnityEngine.UI;

namespace Agava.ServerCheck
{
    public class ServerView : MonoBehaviour
    {
        [SerializeField] private Image _back;
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _serverText;

        public void SetData(string serverName, string status, Color color)
        {
            _serverText.text = serverName;
            _statusText.text = status;
            _back.color = color;
        }
    }
}
