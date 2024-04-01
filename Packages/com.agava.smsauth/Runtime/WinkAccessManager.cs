using System;
using UnityEngine;
using SmsAuthLibrary.DTO;
using SmsAuthLibrary.Program;
//using System.IdentityModel.Tokens.Jwt;

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
        private string _accessToken;
        private string _uniqueId;

        public string AccesToken { get; } = nameof(AccesToken);
        public static IWinkAccessManager Instance {  get; private set; }
        public bool HasAccess { get; private set; } = false;

        public event Action OnRefreshFail;
        public event Action OnSuccessfully;

        private void Start()
        {
            DontDestroyOnLoad(this);
            Instance ??= this;

            if (SmsAuthApi.Initialized == false)
                SmsAuthApi.Initialize(_functionId);

            if (PlayerPrefs.HasKey(UniqueId) == false)
                _uniqueId = Guid.NewGuid().ToString();
            else
                _uniqueId = PlayerPrefs.GetString(UniqueId);

            if (PlayerPrefs.HasKey(AccesToken))
            {
                QuickAccess(new() 
                { 
                    phone = PlayerPrefs.GetString(PhoneNumber), 
                    access_token = PlayerPrefs.GetString(AccesToken)
                });
            }
        }

        public async void Regist(string phoneNumber, Action<bool> otpCodeRequest, Action<bool> winkSubscriptionAccessRequest)
        {
            Debug.Log("Try sign in: " + phoneNumber);
            PlayerPrefs.SetString(PhoneNumber, phoneNumber);

            _winkSubscriptionAccessRequest = winkSubscriptionAccessRequest;
            _data = new()
            {
                phone = phoneNumber,
                otp_code = 0,
                device_id = _uniqueId,
            };

            Response response = await SmsAuthApi.Regist(phoneNumber);

            if (response.statusCode != (uint)YdbStatusCode.Success)
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
                var token = response.body;
                //var handler = new JwtSecurityTokenHandler();
                //JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(token);

                //_accessToken = jwtSecurityToken.EncodedHeader;
                Debug.Log("Otp code match. Token: " + _accessToken);

                if (PlayerPrefs.HasKey(AccesToken) == false)
                    PlayerPrefs.SetString(AccesToken, _accessToken);

                RequestWinkDataBase();
            }
        }

        private async void QuickAccess(SampleAuthData data)
        {
            var response = await SmsAuthApi.SampleAuth(data);

            if(response.statusCode == (uint)StatusCode.ValidationError)
            {
                Debug.LogError("ValidationError : " + response.statusCode);
                OnRefreshFail?.Invoke();
            }
            else
            {
                Debug.Log("Login successfully");
                _accessToken = response.body;
                HasAccess = true;
            }
        }

        private void RequestWinkDataBase() => OnSubscriptionExist(); //TODO: Make Wink request

        private void OnSubscriptionExist()
        {
            //TODO: Make on wink access
            _winkSubscriptionAccessRequest?.Invoke(true);
            HasAccess = true;

            OnSuccessfully?.Invoke();
            Debug.Log("Access succesfully");
        }
    }
}
