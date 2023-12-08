using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for Delivery API settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigDeliveryApi)]
public class DeliveryApiSettings
{
    private const bool StaticEnabled = false;

    private const bool StaticPublicAccess = true;

    private const bool StaticRichTextOutputAsJson = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the Delivery API should be enabled.
    /// </summary>
    /// <value><c>true</c> if the Delivery API should be enabled; otherwise, <c>false</c>.</value>
    [DefaultValue(StaticEnabled)]
    public bool Enabled { get; set; } = StaticEnabled;

    /// <summary>
    ///     Gets or sets a value indicating whether the Delivery API (if enabled) should be
    ///     publicly available or should require an API key for access.
    /// </summary>
    /// <value><c>true</c> if the Delivery API should be publicly available; <c>false</c> if an API key should be required for access.</value>
    [DefaultValue(StaticPublicAccess)]
    public bool PublicAccess { get; set; } = StaticPublicAccess;

    /// <summary>
    ///     Gets or sets the API key used for authorizing API access (if the API is not publicly available) and preview access.
    /// </summary>
    /// <value>A <c>string</c> representing the API key.</value>
    public string? ApiKey { get; set; } = null;

    /// <summary>
    ///     Gets or sets the aliases of the content types that may never be exposed through the Delivery API. Content of these
    ///     types will never be returned from any Delivery API endpoint, nor added to the query index.
    /// </summary>
    /// <value>The content type aliases that are not to be exposed.</value>
    public string[] DisallowedContentTypeAliases { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     Gets or sets a value indicating whether the Delivery API should output rich text values as JSON instead of HTML.
    /// </summary>
    /// <value><c>true</c> if the Delivery API should output rich text values as JSON; <c>false</c> they should be output as HTML (default).</value>
    [DefaultValue(StaticRichTextOutputAsJson)]
    public bool RichTextOutputAsJson { get; set; } = StaticRichTextOutputAsJson;

    /// <summary>
    ///     Gets or sets the settings for the Media APIs of the Delivery API.
    /// </summary>
    public MediaSettings Media { get; set; } = new ();

    /// <summary>
    ///     Gets or sets the member authorization settings for the Delivery API.
    /// </summary>
    public MemberAuthorizationSettings? MemberAuthorization { get; set; } = null;

    /// <summary>
    ///     Gets or sets the settings for the Delivery API output cache.
    /// </summary>
    public OutputCacheSettings OutputCache { get; set; } = new ();

    /// <summary>
    ///     Gets a value indicating if any member authorization type is enabled for the Delivery API.
    /// </summary>
    /// <remarks>
    ///     This method is intended for future extension - see remark in <see cref="MemberAuthorizationSettings"/>.
    /// </remarks>
    public bool MemberAuthorizationIsEnabled() => MemberAuthorization?.AuthorizationCodeFlow?.Enabled is true;

    /// <summary>
    ///     Typed configuration options for the Media APIs of the Delivery API.
    /// </summary>
    /// <remarks>
    ///     The Delivery API settings (as configured in <see cref="DeliveryApiSettings"/>) supersede these settings in levels of restriction.
    ///     I.e. the Media APIs cannot be enabled, if the Delivery API is disabled.
    /// </remarks>
    public class MediaSettings
    {
        /// <summary>
        ///     Gets or sets a value indicating whether the Media APIs of the Delivery API should be enabled.
        /// </summary>
        /// <value><c>true</c> if the Media APIs should be enabled; otherwise, <c>false</c>.</value>
        /// <remarks>
        ///     Setting this to <c>true</c> will have no effect if the Delivery API itself is disabled through <see cref="DeliveryApiSettings"/>
        /// </remarks>
        [DefaultValue(StaticEnabled)]
        public bool Enabled { get; set; } = StaticEnabled;

        /// <summary>
        ///     Gets or sets a value indicating whether the Media APIs of the Delivery API (if enabled) should be
        ///     publicly available or should require an API key for access.
        /// </summary>
        /// <value><c>true</c> if the Media APIs should be publicly available; <c>false</c> if an API key should be required for access.</value>
        /// <remarks>
        ///     Setting this to <c>true</c> will have no effect if the Delivery API itself has public access disabled through <see cref="DeliveryApiSettings"/>
        /// </remarks>
        [DefaultValue(StaticPublicAccess)]
        public bool PublicAccess { get; set; } = StaticPublicAccess;
    }

    /// <summary>
    ///     Typed configuration options for member authorization settings for the Delivery API.
    /// </summary>
    /// <remarks>
    ///     This class is intended for future extension, if/when adding support for additional
    ///     authorization flows (i.e. non-interactive authorization flows).
    /// </remarks>
    public class MemberAuthorizationSettings
    {
        /// <summary>
        ///     Gets or sets the Authorization Code Flow configuration for the Delivery API.
        /// </summary>
        public AuthorizationCodeFlowSettings? AuthorizationCodeFlow { get; set; } = null;
    }

    /// <summary>
    ///     Typed configuration options for the Authorization Code Flow settings for the Delivery API.
    /// </summary>
    public class AuthorizationCodeFlowSettings
    {
        /// <summary>
        ///     Gets or sets a value indicating whether Authorization Code Flow should be enabled for the Delivery API.
        /// </summary>
        /// <value><c>true</c> if Authorization Code Flow should be enabled; otherwise, <c>false</c>.</value>
        [DefaultValue(StaticEnabled)]
        public bool Enabled { get; set; } = StaticEnabled;

        /// <summary>
        ///     Gets or sets the URLs allowed to use as redirect targets after a successful login (session authorization).
        /// </summary>
        /// <value>The URLs allowed as redirect targets.</value>
        public Uri[] LoginRedirectUrls { get; set; } = Array.Empty<Uri>();

        /// <summary>
        ///     Gets or sets the URLs allowed to use as redirect targets after a successful logout (session termination).
        /// </summary>
        /// <value>The URLs allowed as redirect targets.</value>
        /// <remarks>These are only required if logout is to be used.</remarks>
        public Uri[] LogoutRedirectUrls { get; set; } = Array.Empty<Uri>();
    }

    /// <summary>
    ///     Typed configuration options for output caching of the Delivery API.
    /// </summary>
    public class OutputCacheSettings
    {
        private const string StaticDuration = "00:01:00"; // one minute

        /// <summary>
        ///     Gets or sets a value indicating whether the Delivery API output should be cached.
        /// </summary>
        /// <value><c>true</c> if the Delivery API output should be cached; otherwise, <c>false</c>.</value>
        /// <remarks>
        ///     The default value is <c>false</c>.
        /// </remarks>
        [DefaultValue(StaticEnabled)]
        public bool Enabled { get; set; } = StaticEnabled;

        /// <summary>
        ///     Gets or sets a value indicating how long the Content Delivery API output should be cached.
        /// </summary>
        /// <value>Cache lifetime.</value>
        /// <remarks>
        ///     The default cache duration is one minute, if this configuration value is not provided.
        /// </remarks>
        [DefaultValue(StaticDuration)]
        public TimeSpan ContentDuration { get; set; } = TimeSpan.Parse(StaticDuration);

        /// <summary>
        ///     Gets or sets a value indicating how long the Media Delivery API output should be cached.
        /// </summary>
        /// <value>Cache lifetime.</value>
        /// <remarks>
        ///     The default cache duration is one minute, if this configuration value is not provided.
        /// </remarks>
        [DefaultValue(StaticDuration)]
        public TimeSpan MediaDuration { get; set; } = TimeSpan.Parse(StaticDuration);
    }
}
