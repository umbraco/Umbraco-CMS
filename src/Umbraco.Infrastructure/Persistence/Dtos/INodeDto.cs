using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Defines the contract for a DTO that has a node identifier.
/// </summary>
internal interface INodeDto
{
    /// <summary>
    /// Contains the column name constants for <see cref="INodeDto"/>.
    /// </summary>
    internal static class Columns
    {
        /// <summary>
        /// The column name for the node identifier.
        /// </summary>
        internal const string NodeId = Constants.DatabaseSchema.Columns.NodeIdName;
    }

    /// <summary>
    /// Gets or sets the node identifier.
    /// </summary>
    [Column(Columns.NodeId)]
    int NodeId { get; set; }
}
