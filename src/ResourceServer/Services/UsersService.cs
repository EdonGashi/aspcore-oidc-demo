using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Utils.Authorization;
using Utils.Helpers;

namespace ResourceServer.Services
{
    public class UserResult
    {
        [JsonProperty("sub")]
        public string Subject { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("roles")]
        public List<string> Roles { get; set; }
    }

    public interface IUsersService
    {
        Task<UserResult> GetUserAsync(string id);

        Task<IEnumerable<UserResult>> GetUsersAsync();
    }

    public class UsersService : IUsersService
    {
        private readonly IConfiguration configuration;
        private readonly HttpClient httpClient;
        private readonly ITokenProvider tokenProvider;

        public UsersService(IConfiguration configuration, HttpClient httpClient, ITokenProvider tokenProvider)
        {
            this.configuration = configuration;
            this.httpClient = httpClient;
            this.tokenProvider = tokenProvider;
        }

        public async Task<UserResult> GetUserAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var url = PathUtils.Join(Endpoint, "/api/v1/users/", id);
            var response = await GetResourceAsync(url);
            return JsonConvert.DeserializeObject<UserResult>(response);
        }

        public async Task<IEnumerable<UserResult>> GetUsersAsync()
        {
            var url = PathUtils.Join(Endpoint, "/api/v1/users");
            var response = await GetResourceAsync(url);
            return JsonConvert.DeserializeObject<List<UserResult>>(response);
        }

        private string Endpoint => configuration["AuthServer:BaseUrl"];

        private async Task<string> GetResourceAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await tokenProvider.GetTokenAsync());

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
