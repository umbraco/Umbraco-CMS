using System.Runtime.Serialization;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Provides a base class for health checks, filling in the healthcheck metadata on construction
/// </summary>
[DataContract(Name = "healthCheck", Namespace = "")]
public abstract class HealthCheck : IDiscoverable
{
    protected HealthCheck()
    {
        Type thisType = GetType();
        HealthCheckAttribute? meta = thisType.GetCustomAttribute<HealthCheckAttribute>(false);
        if (meta == null)
        {
            throw new InvalidOperationException(
                $"The health check {thisType} requires a {typeof(HealthCheckAttribute)}");
        }

        Name = meta.Name;
        Description = meta.Description;
        Group = meta.Group;
        Id = meta.Id;
    }

    [DataMember(Name = "id")]
    public Guid Id { get; private set; }

    [DataMember(Name = "name")]
    public string Name { get; private set; }

    [DataMember(Name = "description")]
    public string? Description { get; private set; }

    [DataMember(Name = "group")]
    public string? Group { get; private set; }

    /// <summary>
    ///     Get the status for this health check
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     If there are possible actions to take to rectify this check, this method must be overridden by a sub class
    ///     in order to explicitly provide those actions.
    /// </remarks>
    public abstract Task<IEnumerable<HealthCheckStatus>> GetStatus();

    /// <summary>
    ///     Executes the action and returns it's status
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public abstract HealthCheckStatus ExecuteAction(HealthCheckAction action);
}
