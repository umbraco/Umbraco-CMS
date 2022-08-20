namespace Umbraco.Cms.Core.OAuth
{
    public class Token
    {
        public Token(string accessToken, string? refreshToken, DateTime? expiresOn)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresOn = expiresOn;
        }

        public string AccessToken { get; }

        public string? RefreshToken { get; }

        public DateTime? ExpiresOn { get; }

        public bool HasOrIsAboutToExpire =>
            ExpiresOn.HasValue && DateTime.Now.AddSeconds(30) > ExpiresOn;
    }
}
