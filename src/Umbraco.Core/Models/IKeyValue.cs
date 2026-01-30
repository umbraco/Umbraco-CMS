using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a key-value pair entity.
/// </summary>
public interface IKeyValue : IEntity
{
    /// <summary>
    ///     Gets or sets the unique identifier (key) for this entry.
    /// </summary>
    string Identifier { get; set; }

    /// <summary>
    ///     Gets or sets the value associated with the identifier.
    /// </summary>
    string? Value { get; set; }
}
