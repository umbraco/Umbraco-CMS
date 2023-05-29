using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Deploy;

/// <inheritdoc />
/// <remarks>
/// This interface will be merged back into <see cref="IGridCellValueConnector" /> and removed in Umbraco 13.
/// </remarks>
public interface IGridCellValueConnector2 : IGridCellValueConnector
{
    /// <inheritdoc />
    [Obsolete($"Use the overload accepting {nameof(IContextCache)} instead. This overload will be removed in Umbraco 13.")]
    string? IGridCellValueConnector.GetValue(GridValue.GridControl gridControl, ICollection<ArtifactDependency> dependencies)
        => GetValue(gridControl, dependencies, PassThroughCache.Instance);

    /// <summary>
    /// Gets the value to be deployed from the control value as a string.
    /// </summary>
    /// <param name="gridControl">The control containing the value.</param>
    /// <param name="dependencies">The dependencies of the property.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The grid cell value to be deployed.
    /// </returns>
    string? GetValue(GridValue.GridControl gridControl, ICollection<ArtifactDependency> dependencies, IContextCache contextCache);

    /// <inheritdoc />
    [Obsolete($"Use the overload accepting {nameof(IContextCache)} instead. This overload will be removed in Umbraco 13.")]
    void IGridCellValueConnector.SetValue(GridValue.GridControl gridControl)
        => SetValue(gridControl, PassThroughCache.Instance);

    /// <summary>
    /// Allows you to modify the value of a control being deployed.
    /// </summary>
    /// <param name="gridControl">The control being deployed.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <remarks>
    /// Follows the pattern of the property value connectors (<see cref="IValueConnector" />).
    /// The SetValue method is used to modify the value of the <paramref name="gridControl" />.
    /// </remarks>
    void SetValue(GridValue.GridControl gridControl, IContextCache contextCache);
}
