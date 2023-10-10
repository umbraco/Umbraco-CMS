using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

[UmbracoOptions(Constants.Configuration.ConfigWebhook)]
public class WebhookSettings
{
    private const bool StaticEnabled = true;

    /// <summary>
    ///     Gets or sets a value indicating whether webhooks are enabled.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, webhooks are enabled.
    ///         If this option is set to <c>false</c> webhooks will no longer send web-requests.
    ///         the <c>Run</c> level.
    ///     </para>
    /// </remarks>
    [DefaultValue(StaticEnabled)]
    public bool Enabled { get; set; } = StaticEnabled;
}
