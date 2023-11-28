using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

[UmbracoOptions(Constants.Configuration.ConfigWebhook)]
public class WebhookSettings
{
    private const bool StaticEnabled = true;
    private const int StaticMaximumRetries = 5;
    internal const string StaticPeriod = "00:00:10";

    /// <summary>
    ///     Gets or sets a value indicating whether webhooks are enabled.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, webhooks are enabled.
    ///         If this option is set to <c>false</c> webhooks will no longer send web-requests.
    ///     </para>
    /// </remarks>
    [DefaultValue(StaticEnabled)]
    public bool Enabled { get; set; } = StaticEnabled;

    /// <summary>
    ///     Gets or sets a value indicating the maximum number of retries for all webhooks.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, maximum number of retries is 5.
    ///         If this option is set to <c>0</c> webhooks will no longer retry.
    ///     </para>
    /// </remarks>
    [DefaultValue(StaticMaximumRetries)]
    public int MaximumRetries { get; set; } = StaticMaximumRetries;

    /// <summary>
    ///     Gets or sets a value for the period of the webhook firing.
    /// </summary>
    [DefaultValue(StaticPeriod)]
    public TimeSpan Period { get; set; } = TimeSpan.Parse(StaticPeriod);
}
