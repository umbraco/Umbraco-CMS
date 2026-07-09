namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Metadata attribute for Health checks.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class HealthCheckAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HealthCheckAttribute" /> class.
    /// </summary>
    /// <param name="id">The unique identifier for the health check as a GUID string.</param>
    /// <param name="name">The display name of the health check.</param>
    public HealthCheckAttribute(string id, string name)
    {
        Id = new Guid(id);
        Name = name;
    }

    /// <summary>
    ///     Gets the display name of the health check.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets or sets the description of the health check.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the group this health check belongs to.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    ///     Gets the unique identifier of the health check.
    /// </summary>
    public Guid Id { get; }

    // TODO: Do we need more metadata?
}
