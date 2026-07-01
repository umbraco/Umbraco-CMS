using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

/// <summary>
/// Represents the definition of a column within a database index.
/// </summary>
public class IndexColumnDefinition
{
    /// <summary>
    /// Gets or sets the name of the index column.
    /// </summary>
    public virtual string? Name { get; set; }

    /// <summary>
    /// Gets or sets the sort direction (ascending or descending) for the index column.
    /// </summary>
    public virtual Direction Direction { get; set; }
}
