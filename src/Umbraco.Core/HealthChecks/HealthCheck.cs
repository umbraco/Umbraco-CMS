using System.Runtime.Serialization;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Provides a base class for health checks, filling in the healthcheck metadata on construction.
/// </summary>
[DataContract(Name = "healthCheck", Namespace = "")]
public abstract class HealthCheck : IDiscoverable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HealthCheck" /> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the health check does not have a <see cref="HealthCheckAttribute" />.</exception>
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

    /// <summary>
    ///     Gets the unique identifier of the health check.
    /// </summary>
    [DataMember(Name = "id")]
    public Guid Id { get; private set; }

    /// <summary>
    ///     Gets the name of the health check.
    /// </summary>
    [DataMember(Name = "name")]
    public string Name { get; private set; }

    /// <summary>
    ///     Gets the description of the health check.
    /// </summary>
    [DataMember(Name = "description")]
    public string? Description { get; private set; }

    /// <summary>
    ///     Gets the group this health check belongs to.
    /// </summary>
    [DataMember(Name = "group")]
    public string? Group { get; private set; }

    /// <summary>
    ///     Gets the status for this health check.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation containing the collection of health check statuses.</returns>
    /// <remarks>
    ///     If there are possible actions to take to rectify this check, this method must be overridden by a sub class
    ///     in order to explicitly provide those actions.
    /// </remarks>
    public virtual Task<IEnumerable<HealthCheckStatus>> GetStatusAsync() =>
        Task.FromResult(Enumerable.Empty<HealthCheckStatus>());

    /// <summary>
    ///     Executes the action and returns its status.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The health check status after executing the action.</returns>
    public virtual HealthCheckStatus ExecuteAction(HealthCheckAction action) =>
        new HealthCheckStatus("Not implemented");

    /// <summary>
    ///     Executes the action asynchronously and returns its status.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A task that represents the asynchronous operation containing the health check status.</returns>
    public virtual Task<HealthCheckStatus> ExecuteActionAsync(HealthCheckAction action) =>
        Task.FromResult(ExecuteAction(action));
}
