namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Metadata attribute for health check notification methods
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class HealthCheckNotificationMethodAttribute : Attribute
{
    public HealthCheckNotificationMethodAttribute(string alias) => Alias = alias;

    public string Alias { get; }
}
