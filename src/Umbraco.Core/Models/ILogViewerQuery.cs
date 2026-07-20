using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a saved query for the log viewer.
/// </summary>
public interface ILogViewerQuery : IEntity
{
    /// <summary>
    ///     Gets or sets the name of the saved query.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///     Gets or sets the query string expression.
    /// </summary>
    string Query { get; set; }
}
