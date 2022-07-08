namespace Umbraco.Cms.Core.OAuth
{
    public class Token
    {
        public Token(string accessToken, string? refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public string AccessToken { get; }

        public string? RefreshToken { get; }
    }
}
