namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Metadata attribute for health check notification methods.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class HealthCheckNotificationMethodAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HealthCheckNotificationMethodAttribute" /> class.
    /// </summary>
    /// <param name="alias">The alias for the notification method.</param>
    public HealthCheckNotificationMethodAttribute(string alias) => Alias = alias;

    /// <summary>
    ///     Gets the alias for the notification method.
    /// </summary>
    public string Alias { get; }
}
