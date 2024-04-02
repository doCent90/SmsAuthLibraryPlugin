using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using UnityEngine;
using Newtonsoft.Json;
using Agava.Wink;
using SmsAuthLibrary.Program;
using SmsAuthLibrary.Utility;

namespace SmsAuthLibrary.DTO
{
    public static class TokenLifeHelper
    {
        public static bool IsTokenAlive(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            var expiryTime = Convert.ToInt64(jwtToken.Claims.First(claim => claim.Type == "exp").Value);

            DateTime expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(expiryTime).UtcDateTime;

            return expiryDateTime > DateTime.UtcNow;
        }

        public static async Task<string> GetRefreshedToken(string token)
        {
            var refreshResponse = await SmsAuthApi.Refresh(token);

            if (refreshResponse.statusCode != (uint)StatusCode.ValidationError)
            {
                byte[] bytes = Convert.FromBase64String(refreshResponse.body);
                string json = Encoding.UTF8.GetString(bytes);
                var tokensBack = JsonConvert.DeserializeObject<Tokens>(json);

                SaveLoadLocalDataService.Save(tokensBack, WinkAccessManager.Tokens);
                return tokensBack.access;
            }
            else
            {
                Debug.LogError($"Refresh Token Validation Error :{refreshResponse.statusCode}-{refreshResponse.body}");
                return string.Empty;
            }
        }

        public static async Task<bool> IsTokensAlive(Tokens tokens)
        {
            if (IsTokenAlive(tokens.access))
            {
                return true;
            }
            else if (IsTokenAlive(tokens.refresh))
            {
                var currentToken = await GetRefreshedToken(tokens.refresh);

                if (string.IsNullOrEmpty(currentToken))
                    return false;
                else
                    return true;
            }
            else
            {
                SaveLoadLocalDataService.Delete(WinkAccessManager.Tokens);
                return false;
            }
        }

        public static Tokens GetTokens() => SaveLoadLocalDataService.Load<Tokens>(WinkAccessManager.Tokens);
    }
}
