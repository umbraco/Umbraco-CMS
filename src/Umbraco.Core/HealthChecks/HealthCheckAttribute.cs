namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Metadata attribute for Health checks
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class HealthCheckAttribute : Attribute
{
    public HealthCheckAttribute(string id, string name)
    {
        Id = new Guid(id);
        Name = name;
    }

    public string Name { get; }

    public string? Description { get; set; }

    public string? Group { get; set; }

    public Guid Id { get; }

    // TODO: Do we need more metadata?
}
