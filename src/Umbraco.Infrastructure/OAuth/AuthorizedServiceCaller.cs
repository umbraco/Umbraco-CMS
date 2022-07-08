using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.OAuth;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.OAuth
{
    public class AuthorizedServiceCaller : IAuthorizedServiceCaller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly AppCaches _appCaches;
        private readonly ITokenStorage _tokenStorage;
        private readonly AuthorizedServiceSettings _authorizedServiceSettings;

        public AuthorizedServiceCaller(
            IHttpClientFactory httpClientFactory,
            IJsonSerializer jsonSerializer,
            AppCaches appCaches,
            ITokenStorage tokenStorage,
            IOptionsMonitor<AuthorizedServiceSettings> authorizedServiceSettings)
        {
            _httpClientFactory = httpClientFactory;
            _jsonSerializer = jsonSerializer;
            _appCaches = appCaches;
            _tokenStorage = tokenStorage;
            _authorizedServiceSettings = authorizedServiceSettings.CurrentValue;
        }

        public async Task<AuthorizationResult> AuthorizeServiceAsync(string serviceAlias) =>
            await AuthorizeServiceAsync(serviceAlias, new Dictionary<string, string>());

        public async Task<AuthorizationResult> AuthorizeServiceAsync(string serviceAlias, Dictionary<string, string> authorizationParameters)
        {
            ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

            var parameters = new StringBuilder();
            parameters.Append("?client_id=").Append(Uri.EscapeDataString(serviceDetail.ClientId));
            parameters.Append("&client_secret=").Append(Uri.EscapeDataString(serviceDetail.ClientSecret));
            foreach (KeyValuePair<string, string> authorizationParameter in authorizationParameters)
            {
                parameters.Append("&").Append(authorizationParameter.Key).Append("=").Append(Uri.EscapeDataString(authorizationParameter.Value));
            }

            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var url = serviceDetail.TokenHost + serviceDetail.RequestTokenPath + parameters.ToString();
            HttpResponseMessage response = await httpClient.PostAsync(url, new StringContent(parameters.ToString()));
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JObject.Parse(responseContent);

                // Create token from response.
                var accessToken = tokenResponse[serviceDetail.AccessTokenResponseKey]?.ToString();
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new InvalidOperationException($"Could not retrieve access token using key '{serviceDetail.AccessTokenResponseKey}' from the token response from '{serviceAlias}'");
                }

                var refreshToken = tokenResponse[serviceDetail.RefreshTokenResponseKey]?.ToString();
                var token = new Token(accessToken, refreshToken);

                // Add the access token details to the cache.
                var cacheKey = GetTokenCacheKey(serviceAlias);
                _appCaches.RuntimeCache.InsertCacheItem(cacheKey, () => token);

                // Save the refresh token into the storage.
                _tokenStorage.SaveToken(serviceAlias, token);

                return AuthorizationResult.AsSuccess();
            }
            else
            {
                throw new InvalidOperationException($"Error response from token request to '{serviceAlias}'");
            }
        }

        private ServiceDetail GetServiceDetail(string serviceAlias)
        {
            ServiceDetail? serviceDetail = _authorizedServiceSettings.Services.SingleOrDefault(x => x.Alias == serviceAlias);
            if (serviceDetail == null)
            {
                throw new InvalidOperationException($"Cannot find service config for service alias '{serviceAlias}'");
            }

            return serviceDetail;
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(string serviceAlias, string path, HttpMethod httpMethod)
          => await SendRequestAsync<object, TResponse>(serviceAlias, path, httpMethod, null);

        public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent = null)
            where TRequest: class
        {
            ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

            Token? token = GetAccessToken(serviceAlias);
            if (token == null)
            {
                throw new InvalidOperationException($"Cannot request service '{serviceAlias}' as access has not yet been authorized.");
            }

            HttpClient httpClient = _httpClientFactory.CreateClient();

            var requestMessage = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = new Uri(serviceDetail.ApiHost + path),
                Content = GetRequestContent(requestContent)
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("Umbraco", "10.0.0"));

            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                TResponse? result = _jsonSerializer.Deserialize<TResponse>(responseContent);
                if (result != null)
                {
                    return result;
                }

                throw new InvalidOperationException($"Could not deserialize result of request to service '{serviceAlias}'");
            }

            throw new InvalidOperationException($"Error response from '{serviceAlias}'");
        }

        private Token? GetAccessToken(string serviceAlias)
        {
            // First look in cache.
            var cacheKey = GetTokenCacheKey(serviceAlias);
            Token? token = _appCaches.RuntimeCache.GetCacheItem<Token>(cacheKey);
            if (token != null)
            {
                return token;
            }

            // Second, look in storage, and if found, save to cache.
            token = _tokenStorage.GetToken(serviceAlias);
            if (token != null)
            {
                _appCaches.RuntimeCache.InsertCacheItem(cacheKey, () => token);
                return token;
            }

            return null;
        }

        private static string GetTokenCacheKey(string serviceAlias) => $"Umbraco_AuthorizedServiceToken_{serviceAlias}";

        private StringContent? GetRequestContent<TRequest>(TRequest? requestContent)
        {
            if (requestContent == null)
            {
                return null;
            }

            var serializedContent = _jsonSerializer.Serialize(requestContent);
            return new StringContent(serializedContent, Encoding.UTF8, "application/json");
        }

    }
}
