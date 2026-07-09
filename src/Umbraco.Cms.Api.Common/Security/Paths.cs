using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Common.Security;

/// <summary>
///     Contains OAuth/OpenID Connect endpoint paths for Umbraco APIs.
/// </summary>
public static class Paths
{
    /// <summary>
    ///     Contains endpoint paths for back-office authentication.
    /// </summary>
    public static class BackOfficeApi
    {
        /// <summary>
        ///     The base endpoint template for back-office security endpoints.
        /// </summary>
        public const string EndpointTemplate = "security/back-office";

        /// <summary>
        ///     The authorization endpoint path.
        /// </summary>
        public static readonly string AuthorizationEndpoint = EndpointPath($"{EndpointTemplate}/authorize");

        /// <summary>
        ///     The token endpoint path.
        /// </summary>
        public static readonly string TokenEndpoint = EndpointPath($"{EndpointTemplate}/token");

        /// <summary>
        ///     The logout/sign-out endpoint path.
        /// </summary>
        public static readonly string LogoutEndpoint = EndpointPath($"{EndpointTemplate}/signout");

        /// <summary>
        ///     The token revocation endpoint path.
        /// </summary>
        public static readonly string RevokeEndpoint = EndpointPath($"{EndpointTemplate}/revoke");

        private static string EndpointPath(string relativePath) => $"/umbraco{Constants.Web.ManagementApiPath}v1/{relativePath}";
    }

    /// <summary>
    ///     Contains endpoint paths for member authentication.
    /// </summary>
    public static class MemberApi
    {
        /// <summary>
        ///     The base endpoint template for member security endpoints.
        /// </summary>
        public const string EndpointTemplate = "security/member";

        /// <summary>
        ///     The authorization endpoint path.
        /// </summary>
        public static readonly string AuthorizationEndpoint = EndpointPath($"{EndpointTemplate}/authorize");

        /// <summary>
        ///     The token endpoint path.
        /// </summary>
        public static readonly string TokenEndpoint = EndpointPath($"{EndpointTemplate}/token");

        /// <summary>
        ///     The logout/sign-out endpoint path.
        /// </summary>
        public static readonly string LogoutEndpoint = EndpointPath($"{EndpointTemplate}/signout");

        /// <summary>
        ///     The token revocation endpoint path.
        /// </summary>
        public static readonly string RevokeEndpoint = EndpointPath($"{EndpointTemplate}/revoke");

        /// <summary>
        ///     The user info endpoint path.
        /// </summary>
        public static readonly string UserinfoEndpoint = EndpointPath($"{EndpointTemplate}/userinfo");

        // NOTE: we're NOT using /api/v1.0/ here because it will clash with the Delivery API docs
        private static string EndpointPath(string relativePath) => $"/umbraco/delivery/api/v1/{relativePath}";
    }
}
