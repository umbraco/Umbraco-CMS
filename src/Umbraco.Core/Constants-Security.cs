namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class Security
    {
        /// <summary>
        ///     Gets the identifier of the 'super' user.
        /// </summary>
        public const int SuperUserId = -1;

        public const string SuperUserIdAsString = "-1";

        /// <summary>
        ///     The id for the 'unknown' user.
        /// </summary>
        /// <remarks>
        ///     This is a user row that exists in the DB only for referential integrity but the user is never returned from any of
        ///     the services
        /// </remarks>
        public const int UnknownUserId = 0;

        /// <summary>
        ///     The name of the 'unknown' user.
        /// </summary>
        public const string UnknownUserName = "SYSTEM";

        public const string AdminGroupAlias = "admin";
        public const string EditorGroupAlias = "editor";
        public const string SensitiveDataGroupAlias = "sensitiveData";
        public const string TranslatorGroupAlias = "translator";
        public const string WriterGroupAlias = "writer";

        public const string BackOfficeAuthenticationType = "UmbracoBackOffice";
        public const string BackOfficeExternalAuthenticationType = "UmbracoExternalCookie";
        public const string BackOfficeExternalCookieName = "UMB_EXTLOGIN";
        public const string BackOfficeTokenAuthenticationType = "UmbracoBackOfficeToken";
        public const string BackOfficeTwoFactorAuthenticationType = "UmbracoTwoFactorCookie";
        public const string BackOfficeTwoFactorRememberMeAuthenticationType = "UmbracoTwoFactorRememberMeCookie";

        public const string EmptyPasswordPrefix = "___UIDEMPTYPWORD__";

        public const string DefaultMemberTypeAlias = "Member";

        /// <summary>
        ///     The prefix used for external identity providers for their authentication type
        /// </summary>
        /// <remarks>
        ///     By default we don't want to interfere with front-end external providers and their default setup, for back office
        ///     the
        ///     providers need to be setup differently and each auth type for the back office will be prefixed with this value
        /// </remarks>
        public const string BackOfficeExternalAuthenticationTypePrefix = "Umbraco.";

        public const string MemberExternalAuthenticationTypePrefix = "UmbracoMembers.";

        public const string StartContentNodeIdClaimType =
            "http://umbraco.org/2015/02/identity/claims/backoffice/startcontentnode";

        public const string StartMediaNodeIdClaimType =
            "http://umbraco.org/2015/02/identity/claims/backoffice/startmedianode";

        public const string AllowedApplicationsClaimType =
            "http://umbraco.org/2015/02/identity/claims/backoffice/allowedapp";

        public const string SessionIdClaimType = "http://umbraco.org/2015/02/identity/claims/backoffice/sessionid";

        public const string TicketExpiresClaimType =
            "http://umbraco.org/2020/06/identity/claims/backoffice/ticketexpires";

        /// <summary>
        ///     The claim type for the ASP.NET Identity security stamp
        /// </summary>
        public const string SecurityStampClaimType = "AspNet.Identity.SecurityStamp";

        public const string AspNetCoreV3PasswordHashAlgorithmName = "PBKDF2.ASPNETCORE.V3";
        public const string AspNetCoreV2PasswordHashAlgorithmName = "PBKDF2.ASPNETCORE.V2";
        public const string AspNetUmbraco8PasswordHashAlgorithmName = "HMACSHA256";
        public const string AspNetUmbraco4PasswordHashAlgorithmName = "HMACSHA1";
        public const string UnknownPasswordConfigJson = "{\"hashAlgorithm\":\"Unknown\"}";
    }
}
