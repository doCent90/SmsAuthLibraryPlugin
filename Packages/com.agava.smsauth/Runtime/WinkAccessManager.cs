using System;
using UnityEngine;
using SmsAuthLibrary.DTO;
using SmsAuthLibrary.Program;
using System.Text;
using Newtonsoft.Json;
using Utility;

namespace Agava.Wink
{
    [DefaultExecutionOrder(100)]
    public class WinkAccessManager : MonoBehaviour, IWinkAccessManager
    {
        private const string UniqueId = nameof(UniqueId);
        private const string PhoneNumber = nameof(PhoneNumber);

        [SerializeField] private string _functionId;

        private LoginData _data;
        private Action<bool> _winkSubscriptionAccessRequest;
        private string _uniqueId;

        public static string Tokens { get; } = nameof(Tokens);
        public bool HasAccess { get; private set; } = false;
        public static IWinkAccessManager Instance {  get; private set; }

        public event Action OnRefreshFail;
        public event Action OnSuccessfully;

        private void Awake()
        {
            Instance ??= this;            
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            if (SmsAuthApi.Initialized == false)
                SmsAuthApi.Initialize(_functionId);

            if (PlayerPrefs.HasKey(UniqueId) == false)
                _uniqueId = Guid.NewGuid().ToString();
            else
                _uniqueId = PlayerPrefs.GetString(UniqueId);

            if (PlayerPrefs.HasKey(Tokens))
                QuickAccess();
        }

        public async void Regist(string phoneNumber, Action<bool> otpCodeRequest, Action<bool> winkSubscriptionAccessRequest)
        {
            PlayerPrefs.SetString(PhoneNumber, phoneNumber);

            _winkSubscriptionAccessRequest = winkSubscriptionAccessRequest;
            _data = new()
            {
                phone = phoneNumber,
                otp_code = 0,
                device_id = _uniqueId,
            };

            Response response = await SmsAuthApi.Regist(phoneNumber);

            if (response.statusCode != (uint)YbdStatusCode.Success)
            {
                otpCodeRequest?.Invoke(false);
                Debug.LogError("Error : " + response.statusCode);
            }
            else
            {
                otpCodeRequest?.Invoke(true);
            }
        }

        public void SendOtpCode(uint enteredOtpCode)
        {
            _data.otp_code = enteredOtpCode;
            Login(_data);
        }

        internal void TestEnableSubsription() => OnSubscriptionExist();

        private async void Login(LoginData data)
        {
            var response = await SmsAuthApi.Login(data);

            if (response.statusCode == (uint)StatusCode.ValidationError)
            {
                Debug.LogError("ValidationError : " + response.statusCode);
                _winkSubscriptionAccessRequest?.Invoke(false);
            }
            else
            {
                string token;

                if (response.isBase64Encoded)
                {
                    byte[] bytes = Convert.FromBase64String(response.body);
                    token = Encoding.UTF8.GetString(bytes);
                }
                else
                {
                    token = response.body;
                }

                Tokens tokens = JsonConvert.DeserializeObject<Tokens>(token);
                SaveLoadLocalDataService.Save(tokens, Tokens);
                RequestWinkDataBase();
            }
        }

        private async void QuickAccess()
        {
            var tokens = SaveLoadLocalDataService.Load<Tokens>(Tokens);

            if(tokens == null)
            {
                Debug.LogError("Tokens not exhist");
                OnRefreshFail?.Invoke();
                return;
            }

            string currentToken = string.Empty;

            if (TokenLifeHelper.IsTokenAlive(tokens.access))
            {
                currentToken = tokens.access;
            }
            else if (TokenLifeHelper.IsTokenAlive(tokens.refresh))
            {
                currentToken = await TokenLifeHelper.GetRefreshedToken(tokens.refresh);

                if(string.IsNullOrEmpty(currentToken))
                {
                    OnRefreshFail?.Invoke();
                    return;
                }
            }
            else
            {
                OnRefreshFail?.Invoke();
                SaveLoadLocalDataService.Delete(Tokens);
                return;
            }

            var response = await SmsAuthApi.SampleAuth(currentToken);

            if(response.statusCode != (uint)StatusCode.ValidationError)
            {
                OnSubscriptionExist();
            }
            else
            {
                Debug.LogError($"Quick access Validation Error: {response.body}-code: {response.statusCode}");
                OnRefreshFail?.Invoke();
            }
        }

        private void RequestWinkDataBase() //TODO: Make Wink request
        {
            _winkSubscriptionAccessRequest?.Invoke(true);
            OnSubscriptionExist(); 
        }

        private void OnSubscriptionExist()
        {
            HasAccess = true;
            OnSuccessfully?.Invoke();
            Debug.Log("Access succesfully");
        }
    }
}
