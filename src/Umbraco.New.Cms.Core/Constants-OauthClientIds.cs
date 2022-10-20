namespace Umbraco.New.Cms.Core;

// TODO: move this class to Umbraco.Cms.Core as a partial class
public static class Constants
{
    public static partial class OauthClientIds
    {
        /// <summary>
        /// Client ID used for default back-office access
        /// </summary>
        public const string BackOffice = "umbraco-back-office";

        /// <summary>
        /// Client ID used for Swagger API access
        /// </summary>
        public const string Swagger = "umbraco-swagger";

        /// <summary>
        /// Client ID used for Postman API access
        /// </summary>
        public const string Postman = "umbraco-postman";
    }
}
