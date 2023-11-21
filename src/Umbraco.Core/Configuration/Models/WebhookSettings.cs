using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

[UmbracoOptions(Constants.Configuration.ConfigWebhook)]
public class WebhookSettings
{
    private const bool StaticEnabled = true;
    private const int StaticMaximumRetries = 5;
    internal const string StaticPeriod = "00:00:10";
    private const bool StaticEnableLoggingCleanup = true;
    private const int StaticKeepLogsForDays = 30;


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

    /// <summary>
    ///     Gets or sets a value indicating whether cleanup of webhook logs are enabled.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, cleanup is enabled.
    ///     </para>
    /// </remarks>
    [DefaultValue(StaticEnableLoggingCleanup)]
    public bool EnableLoggingCleanup { get; set; } = StaticEnableLoggingCleanup;

    /// <summary>
    ///     Gets or sets a value indicating number of days to keep logs for.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, logs are kept for 30 days.
    ///     </para>
    /// </remarks>
    [DefaultValue(StaticKeepLogsForDays)]
    public int KeepLogsForDays { get; set; } = StaticKeepLogsForDays;
}
