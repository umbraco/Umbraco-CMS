using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

[UmbracoOptions(Constants.Configuration.ConfigWebhook)]
public class WebhookSettings
{
    private const bool StaticEnabled = true;
    private const int StaticMaximumRetries = 5;

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
    ///     Gets or sets a value indicating the increments of the delay between retries..
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, maximum number of retries is 5, and thus this array also have 5 values in it.
    ///         If  the maximum number of retries is higher than the counter of the delay, the last value in the array is used.
    ///     </para>
    /// </remarks>
    public int[] RetryDelaysInMilliseconds { get; set; } = { 500, 1000, 2000, 4000, 8000 };
}
