namespace Umbraco.Cms.Core.OAuth
{
    public class AuthorizationResult
    {
        private AuthorizationResult()
        {
        }

        public bool Success { get; private set; }

        public string ErrorMessage { get; private set; } = string.Empty;

        public static AuthorizationResult AsSuccess() => new AuthorizationResult { Success = true };

        public static AuthorizationResult AsError(string message) => new AuthorizationResult { ErrorMessage = message };

    }
}
