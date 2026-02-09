namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Security-related constants including user identifiers, authentication types, and claim types.
    /// </summary>
    public static class Security
    {
        /// <summary>
        ///     Gets the identifier of the 'super' user.
        /// </summary>
        [Obsolete("Use SuperUserKey instead. Scheduled for removal in V15.")]
        public const int SuperUserId = -1;

        /// <summary>
        /// Gets the unique key of the 'super' user.
        /// </summary>
        public static readonly Guid SuperUserKey = new("1E70F841-C261-413B-ABB2-2D68CDB96094");

        /// <summary>
        ///     Gets the identifier of the 'super' user as a string.
        /// </summary>
        [Obsolete("Use SuperUserKey instead. Scheduled for removal in V15.")]
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

        /// <summary>
        ///     The alias of the admin user group.
        /// </summary>
        public const string AdminGroupAlias = "admin";

        /// <summary>
        /// The key of the admin group
        /// </summary>
        public static readonly Guid AdminGroupKey = new(AdminGroupKeyString);

        /// <summary>
        ///     The key of the admin group as a string.
        /// </summary>
        internal const string AdminGroupKeyString = "E5E7F6C8-7F9C-4B5B-8D5D-9E1E5A4F7E4D";


        /// <summary>
        /// The key of the editor group
        /// </summary>
        public static readonly Guid EditorGroupKey = new(EditorGroupKeyString);

        /// <summary>
        ///     The key of the editor group as a string.
        /// </summary>
        internal const string EditorGroupKeyString = "44DC260E-B4D4-4DD9-9081-EEC5598F1641";


        /// <summary>
        /// The key of the sensitive data group
        /// </summary>
        public static readonly Guid SensitiveDataGroupKey = new(SensitiveDataGroupKeyString);

        /// <summary>
        ///     The key of the sensitive data group as a string.
        /// </summary>
        internal const string SensitiveDataGroupKeyString = "8C6AD70F-D307-4E4A-AF58-72C2E4E9439D";

        /// <summary>
        /// The key of the translator group
        /// </summary>
        public static readonly Guid TranslatorGroupKey = new(TranslatorGroupString);

        /// <summary>
        ///     The key of the translator group as a string.
        /// </summary>
        internal const string TranslatorGroupString = "F2012E4C-D232-4BD1-8EAE-4384032D97D8";

        /// <summary>
        /// The key of the writer group
        /// </summary>
        public static readonly Guid WriterGroupKey = new(WriterGroupKeyString);

        /// <summary>
        ///     The key of the writer group as a string.
        /// </summary>
        internal const string WriterGroupKeyString = "9FC2A16F-528C-46D6-A014-75BF4EC2480C";


        /// <summary>
        ///     The authentication type for backoffice authentication.
        /// </summary>
        public const string BackOfficeAuthenticationType = "UmbracoBackOffice";

        /// <summary>
        ///     The authentication type for backoffice external login providers.
        /// </summary>
        public const string BackOfficeExternalAuthenticationType = "UmbracoExternalCookie";

        /// <summary>
        ///     The cookie name for backoffice external login authentication.
        /// </summary>
        public const string BackOfficeExternalCookieName = "UMB_EXTLOGIN";

        /// <summary>
        ///     The authentication type for backoffice token-based authentication.
        /// </summary>
        public const string BackOfficeTokenAuthenticationType = "UmbracoBackOfficeToken";

        /// <summary>
        ///     The authentication type for backoffice two-factor authentication.
        /// </summary>
        public const string BackOfficeTwoFactorAuthenticationType = "UmbracoTwoFactorCookie";

        /// <summary>
        ///     The authentication type for backoffice two-factor "remember me" functionality.
        /// </summary>
        public const string BackOfficeTwoFactorRememberMeAuthenticationType = "UmbracoTwoFactorRememberMeCookie";

        /// <summary>
        /// Authentication type and scheme used for backoffice users when it is exposed out of the backoffice context via a cookie.
        /// </summary>
        public const string BackOfficeExposedAuthenticationType = "UmbracoBackOfficeExposed";

        /// <summary>
        /// Represents the name of the authentication cookie used to expose the backoffice authentication token outside of the backoffice context.
        /// </summary>
        public const string BackOfficeExposedCookieName = "UMB_UCONTEXT_EXPOSED";

        /// <summary>
        ///     The prefix used to identify empty password placeholders.
        /// </summary>
        public const string EmptyPasswordPrefix = "___UIDEMPTYPWORD__";

        /// <summary>
        ///     The default member type alias used when no specific member type is specified.
        /// </summary>
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

        /// <summary>
        ///     The prefix used for member external authentication types.
        /// </summary>
        public const string MemberExternalAuthenticationTypePrefix = "UmbracoMembers.";

        /// <summary>
        ///     The claim type for the user's start content node ID.
        /// </summary>
        [Obsolete("Please use the UserExtensions class to access user start node info. Will be removed in V15.")]
        public const string StartContentNodeIdClaimType =
            "http://umbraco.org/2015/02/identity/claims/backoffice/startcontentnode";

        /// <summary>
        ///     The claim type for the user's start media node ID.
        /// </summary>
        [Obsolete("Please use the UserExtensions class to access user start node info. Will be removed in V15.")]
        public const string StartMediaNodeIdClaimType =
            "http://umbraco.org/2015/02/identity/claims/backoffice/startmedianode";

        /// <summary>
        ///     The claim type for the user's allowed applications.
        /// </summary>
        [Obsolete("Please use IUser.AllowedSections instead. Will be removed in V15.")]
        public const string AllowedApplicationsClaimType =
            "http://umbraco.org/2015/02/identity/claims/backoffice/allowedapp";

        /// <summary>
        ///     The claim type for the backoffice session ID.
        /// </summary>
        public const string SessionIdClaimType = "http://umbraco.org/2015/02/identity/claims/backoffice/sessionid";

        /// <summary>
        ///     The claim type for the authentication ticket expiration time.
        /// </summary>
        public const string TicketExpiresClaimType =
            "http://umbraco.org/2020/06/identity/claims/backoffice/ticketexpires";

        /// <summary>
        ///     The claim type for the ASP.NET Identity security stamp
        /// </summary>
        public const string SecurityStampClaimType = "AspNet.Identity.SecurityStamp";

        /// <summary>
        ///     The claim type for the mandatory OpenIdDict sub claim
        /// </summary>
        public const string OpenIdDictSubClaimType = "sub";

        /// <summary>
        ///     The algorithm name for ASP.NET Core v3 PBKDF2 password hashing.
        /// </summary>
        public const string AspNetCoreV3PasswordHashAlgorithmName = "PBKDF2.ASPNETCORE.V3";

        /// <summary>
        ///     The algorithm name for ASP.NET Core v2 PBKDF2 password hashing.
        /// </summary>
        public const string AspNetCoreV2PasswordHashAlgorithmName = "PBKDF2.ASPNETCORE.V2";

        /// <summary>
        ///     The algorithm name for Umbraco 8 HMACSHA256 password hashing.
        /// </summary>
        public const string AspNetUmbraco8PasswordHashAlgorithmName = "HMACSHA256";

        /// <summary>
        ///     The algorithm name for Umbraco 4 HMACSHA1 password hashing.
        /// </summary>
        public const string AspNetUmbraco4PasswordHashAlgorithmName = "HMACSHA1";

        /// <summary>
        ///     The JSON configuration string for unknown password hash algorithms.
        /// </summary>
        public const string UnknownPasswordConfigJson = "{\"hashAlgorithm\":\"Unknown\"}";
    }
}
