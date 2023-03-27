using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Extension methods adding backwards-compatability between <see cref="IGridCellValueConnector" /> and <see cref="IGridCellValueConnector2" />.
/// </summary>
/// <remarks>
/// These extension methods will be removed in Umbraco 13.
/// </remarks>
[Obsolete("The grid is obsolete, will be removed in V13")]
public static class GridCellValueConnectorExtensions
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <param name="gridControl">The grid control.</param>
    /// <param name="dependencies">The dependencies.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The value.
    /// </returns>
    /// <remarks>
    /// This extension method tries to make use of the <see cref="IContextCache" /> on types also implementing <see cref="IGridCellValueConnector2" />.
    /// </remarks>
    public static string? GetValue(this IGridCellValueConnector connector, GridValue.GridControl gridControl, ICollection<ArtifactDependency> dependencies, IContextCache contextCache)
        => connector is IGridCellValueConnector2 connector2
            ? connector2.GetValue(gridControl, dependencies, contextCache)
            : connector.GetValue(gridControl, dependencies);

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <param name="gridControl">The grid control.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <remarks>
    /// This extension method tries to make use of the <see cref="IContextCache" /> on types also implementing <see cref="IGridCellValueConnector2" />.
    /// </remarks>
    public static void SetValue(this IGridCellValueConnector connector, GridValue.GridControl gridControl, IContextCache contextCache)
    {
        if (connector is IGridCellValueConnector2 connector2)
        {
            connector2.SetValue(gridControl, contextCache);
        }
        else
        {
            connector.SetValue(gridControl);
        }
    }
}
