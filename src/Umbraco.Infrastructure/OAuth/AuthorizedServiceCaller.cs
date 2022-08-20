using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AuthorizedServiceCaller> _logger;
        private readonly AuthorizedServiceSettings _authorizedServiceSettings;

        public AuthorizedServiceCaller(
            IHttpClientFactory httpClientFactory,
            IJsonSerializer jsonSerializer,
            AppCaches appCaches,
            ITokenStorage tokenStorage,
            ILogger<AuthorizedServiceCaller> logger,
            IOptionsMonitor<AuthorizedServiceSettings> authorizedServiceSettings)
        {
            _httpClientFactory = httpClientFactory;
            _jsonSerializer = jsonSerializer;
            _appCaches = appCaches;
            _tokenStorage = tokenStorage;
            _logger = logger;
            _authorizedServiceSettings = authorizedServiceSettings.CurrentValue;
        }

        public async Task<AuthorizationResult> AuthorizeServiceAsync(string serviceAlias, string authorizationCode, string redirectUri)
        {
            ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

            Dictionary<string, string> parameters = BuildAuthorizationParameters(serviceDetail, authorizationCode, redirectUri);

            HttpResponseMessage response = await SendAuthorizationRequest(serviceDetail, parameters);
            if (response.IsSuccessStatusCode)
            {
                Token token = await CreateTokenFromResponse(serviceAlias, serviceDetail, response);

                StoreToken(serviceAlias, token);

                return AuthorizationResult.AsSuccess();
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error response from token request to '{serviceAlias}'. Response: {responseContent}");
            }
        }

        private void StoreToken(string serviceAlias, Token token)
        {
            // Add the access token details to the cache.
            var cacheKey = GetTokenCacheKey(serviceAlias);
            _appCaches.RuntimeCache.InsertCacheItem(cacheKey, () => token);

            // Save the refresh token into the storage.
            _tokenStorage.SaveToken(serviceAlias, token);
        }

        private static async Task<Token> CreateTokenFromResponse(string serviceAlias, ServiceDetail serviceDetail, HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JObject.Parse(responseContent);

            var accessToken = tokenResponse[serviceDetail.AccessTokenResponseKey]?.ToString();
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException($"Could not retrieve access token using key '{serviceDetail.AccessTokenResponseKey}' from the token response from '{serviceAlias}'");
            }

            var refreshToken = tokenResponse[serviceDetail.RefreshTokenResponseKey]?.ToString();

            DateTime? expiresOn = null;
            var expiresInValue = tokenResponse[serviceDetail.ExpiresInResponseKey]?.ToString();
            if (!string.IsNullOrEmpty(expiresInValue))
            {
                var expiresInSeconds = int.Parse(expiresInValue);
                expiresOn = DateTime.Now.AddSeconds(expiresInSeconds);
            }

            return new Token(accessToken, refreshToken, expiresOn);
        }

        private Dictionary<string, string> BuildAuthorizationParameters(ServiceDetail serviceDetail, string authorizationCode, string redirectUri) =>
            new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "client_id", serviceDetail.ClientId },
                    { "client_secret", serviceDetail.ClientSecret },
                    { "code", authorizationCode },
                    { "redirect_uri", redirectUri }
                };

        private ServiceDetail GetServiceDetail(string serviceAlias)
        {
            ServiceDetail? serviceDetail = _authorizedServiceSettings.Services.SingleOrDefault(x => x.Alias == serviceAlias);
            if (serviceDetail == null)
            {
                throw new InvalidOperationException($"Cannot find service config for service alias '{serviceAlias}'");
            }

            return serviceDetail;
        }

        private async Task<HttpResponseMessage> SendAuthorizationRequest(ServiceDetail serviceDetail, Dictionary<string, string> parameters)
        {
            HttpClient httpClient = CreateAuthorizationClient();

            var url = serviceDetail.TokenHost + serviceDetail.RequestTokenPath;

            HttpContent? content = null;
            switch (serviceDetail.RequestTokenFormat)
            {
                case TokenRequestContentFormat.Querystring:
                    url += BuildAuthorizationQuerystring(parameters);
                    break;
                case TokenRequestContentFormat.FormUrlEncoded:
                    content = new FormUrlEncodedContent(parameters);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serviceDetail.RequestTokenFormat));
            }

            return await httpClient.PostAsync(url, content);
        }

        private static string BuildAuthorizationQuerystring(Dictionary<string, string> parameters)
        {
            var qs = new StringBuilder();
            var sep = "?";
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                qs.Append(sep).Append(parameter.Key).Append("=").Append(parameter.Value);
                sep = "&";
            }

            return qs.ToString();
        }

        private HttpClient CreateAuthorizationClient()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(string serviceAlias, string path, HttpMethod httpMethod)
          => await SendRequestAsync<object, TResponse>(serviceAlias, path, httpMethod, null);

        public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent = null)
            where TRequest : class
        {
            ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

            Token? token = GetAccessToken(serviceAlias);
            if (token == null)
            {
                throw new InvalidOperationException($"Cannot request service '{serviceAlias}' as access has not yet been authorized.");
            }

            token = await EnsureAccessToken(serviceAlias, token);

            HttpClient httpClient = _httpClientFactory.CreateClient();

            HttpRequestMessage requestMessage = CreateRequestMessage(serviceDetail, path, httpMethod, token, requestContent);

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

        private async Task<Token> EnsureAccessToken(string serviceAlias, Token token)
        {
            if (token.HasOrIsAboutToExpire)
            {
                if (string.IsNullOrEmpty(token.RefreshToken))
                {
                    ClearAccessToken(serviceAlias);
                    throw new InvalidOperationException($"Cannot request service '{serviceAlias}' as the access token has expired and no refresh token is available to use. The expired token has been deleted.");
                }

                Token? refreshedToken = await RefreshAccessToken(serviceAlias, token.RefreshToken);

                if (refreshedToken == null)
                {
                    throw new InvalidOperationException($"Cannot request service '{serviceAlias}' as the access token has expired and the refresh token could not be used to obtain a new access token.");
                }

                return refreshedToken;
            }

            return token;
        }

        private async Task<Token?> RefreshAccessToken(string serviceAlias, string refreshToken)
        {
            ServiceDetail serviceDetail = GetServiceDetail(serviceAlias);

            Dictionary<string, string> parameters = BuildRefreshTokenParameters(serviceDetail, refreshToken);

            HttpResponseMessage response = await SendAuthorizationRequest(serviceDetail, parameters);
            if (response.IsSuccessStatusCode)
            {
                Token token = await CreateTokenFromResponse(serviceAlias, serviceDetail, response);

                StoreToken(serviceAlias, token);

                return token;
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error response from refresh token request to '{serviceAlias}'. Response: {responseContent}");
            }
        }

        private Dictionary<string, string> BuildRefreshTokenParameters(ServiceDetail serviceDetail, string refreshToken) =>
            new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "client_id", serviceDetail.ClientId },
                    { "client_secret", serviceDetail.ClientSecret },
                    { "refresh_token", refreshToken }
                };

        private HttpRequestMessage CreateRequestMessage<TRequest>(ServiceDetail serviceDetail, string path, HttpMethod httpMethod, Token token, TRequest? requestContent)
            where TRequest : class
        {
            var requestMessage = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = new Uri(serviceDetail.ApiHost + path),
                Content = GetRequestContent(requestContent)
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("Umbraco", "10.0.0"));
            return requestMessage;
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

        private void ClearAccessToken(string serviceAlias)
        {
            _appCaches.RuntimeCache.ClearByKey(GetTokenCacheKey(serviceAlias));
            _tokenStorage.DeleteToken(serviceAlias);
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
