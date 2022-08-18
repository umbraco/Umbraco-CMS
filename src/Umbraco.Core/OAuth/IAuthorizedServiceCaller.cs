namespace Umbraco.Cms.Core.OAuth
{
    public interface IAuthorizedServiceCaller
    {
        Task<AuthorizationResult> AuthorizeServiceAsync(string serviceAlias, string authorizationCode, string redirectUri);

        Task<TResponse> SendRequestAsync<TResponse>(string serviceAlias, string path, HttpMethod httpMethod);

        Task<TResponse> SendRequestAsync<TRequest, TResponse>(string serviceAlias, string path, HttpMethod httpMethod, TRequest? requestContent = null)
            where TRequest : class;
    }
}
