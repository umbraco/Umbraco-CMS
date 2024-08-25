namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class OAuthClientIds
    {
        /// <summary>
        ///     Client ID used for default back-office access.
        /// </summary>
        public const string BackOffice = "umbraco-back-office";

        /// <summary>
        ///     Client ID used for Swagger API access.
        /// </summary>
        public const string Swagger = "umbraco-swagger";

        /// <summary>
        ///     Client ID used for Postman API access.
        /// </summary>
        public const string Postman = "umbraco-postman";

        /// <summary>
        ///     Client ID used for member access.
        /// </summary>
        public const string Member = "umbraco-member";
    }
}
