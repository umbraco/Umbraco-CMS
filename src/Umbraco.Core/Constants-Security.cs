namespace Umbraco.Cms.Core;

public static partial class Constants
{
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

        public const string AdminGroupAlias = "admin";

        [Obsolete("Use EditorGroupKey instead. Scheduled for removal in V15.")]
        public const string EditorGroupAlias = "editor";

        [Obsolete("Use SensitiveDataGroupKey instead. Scheduled for removal in V15.")]
        public const string SensitiveDataGroupAlias = "sensitiveData";

        [Obsolete("Use TranslatorGroupKey instead. Scheduled for removal in V15.")]
        public const string TranslatorGroupAlias = "translator";

        [Obsolete("Use WriterGroupKey instead. Scheduled for removal in V15.")]
        public const string WriterGroupAlias = "writer";

        /// <summary>
        /// The key of the admin group
        /// </summary>
        public static readonly Guid AdminGroupKey = new(AdminGroupKeyString);
        internal const string AdminGroupKeyString = "E5E7F6C8-7F9C-4B5B-8D5D-9E1E5A4F7E4D";


        /// <summary>
        /// The key of the editor group
        /// </summary>
        public static readonly Guid EditorGroupKey = new(EditorGroupKeyString);
        internal const string EditorGroupKeyString = "44DC260E-B4D4-4DD9-9081-EEC5598F1641";


        /// <summary>
        /// The key of the sensitive data group
        /// </summary>
        public static readonly Guid SensitiveDataGroupKey = new(SensitiveDataGroupKeyString);
        internal const string SensitiveDataGroupKeyString = "8C6AD70F-D307-4E4A-AF58-72C2E4E9439D";

        /// <summary>
        /// The key of the translator group
        /// </summary>
        public static readonly Guid TranslatorGroupKey = new(TranslatorGroupString);
        internal const string TranslatorGroupString = "F2012E4C-D232-4BD1-8EAE-4384032D97D8";

        /// <summary>
        /// The key of the writer group
        /// </summary>
        public static readonly Guid WriterGroupKey = new(WriterGroupKeyString);
        internal const string WriterGroupKeyString = "9FC2A16F-528C-46D6-A014-75BF4EC2480C";


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

        [Obsolete("Please use the UserExtensions class to access user start node info. Will be removed in V15.")]
        public const string StartContentNodeIdClaimType =
            "http://umbraco.org/2015/02/identity/claims/backoffice/startcontentnode";

        [Obsolete("Please use the UserExtensions class to access user start node info. Will be removed in V15.")]
        public const string StartMediaNodeIdClaimType =
            "http://umbraco.org/2015/02/identity/claims/backoffice/startmedianode";

        [Obsolete("Please use IUser.AllowedSections instead. Will be removed in V15.")]
        public const string AllowedApplicationsClaimType =
            "http://umbraco.org/2015/02/identity/claims/backoffice/allowedapp";

        public const string SessionIdClaimType = "http://umbraco.org/2015/02/identity/claims/backoffice/sessionid";

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

        public const string AspNetCoreV3PasswordHashAlgorithmName = "PBKDF2.ASPNETCORE.V3";
        public const string AspNetCoreV2PasswordHashAlgorithmName = "PBKDF2.ASPNETCORE.V2";
        public const string AspNetUmbraco8PasswordHashAlgorithmName = "HMACSHA256";
        public const string AspNetUmbraco4PasswordHashAlgorithmName = "HMACSHA1";
        public const string UnknownPasswordConfigJson = "{\"hashAlgorithm\":\"Unknown\"}";
    }
}
