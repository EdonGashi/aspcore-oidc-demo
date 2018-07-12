using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Utils.Authorization
{
    public class ClientCredentialsAuthenticator : ITokenProvider
    {
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly HttpClient httpClient;
        private readonly string tokenEndpoint;

        private string currentToken;
        private DateTime expiryDate;

        public ClientCredentialsAuthenticator(string clientId, string clientSecret, HttpClient httpClient, string tokenEndpoint)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.httpClient = httpClient;
            this.tokenEndpoint = tokenEndpoint;
        }

        public async Task<string> GetTokenAsync()
        {
            if (currentToken == null || DateTime.UtcNow > expiryDate)
            {
                currentToken = await GetNewTokenAsync();
            }

            return currentToken;
        }

        private async Task<string> GetNewTokenAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret
                })
            };

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();

            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (payload["error"] != null)
            {
                throw new InvalidOperationException("An error occurred while retrieving an access token.");
            }

            var expiresIn = 10d * 60d;
            var expiresInToken = payload["expires_in"];
            if (expiresInToken != null)
            {
                expiresIn = expiresInToken.ToObject<double>();
            }

            expiryDate = DateTime.UtcNow + TimeSpan.FromSeconds(0.9d * expiresIn);
            return (string)payload["access_token"];
        }
    }
}
