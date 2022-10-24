using Umbraco.Cms.Core.Models;
using static Umbraco.Cms.Core.Models.GridValue;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Defines methods that can convert a grid cell value to / from an environment-agnostic string.
/// </summary>
/// <remarks>
/// Grid cell values may contain values such as content identifiers, that would be local
/// to one environment, and need to be converted in order to be deployed.
/// </remarks>
public interface IGridCellValueConnector
{
    /// <summary>
    /// Gets a value indicating whether the connector supports a specified grid editor view.
    /// </summary>
    /// <param name="view">The grid editor view. It needs to be the view instead of the alias as the view is really what
    /// identifies what kind of connector should be used. Alias can be anything and you can have multiple different aliases
    /// using the same kind of view.</param>
    /// <returns>
    ///   <c>true</c> if the specified view is connector; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// A value indicating whether the connector supports the grid editor view.
    /// </remarks>
    bool IsConnector(string view);

    /// <summary>
    /// Gets the value to be deployed from the control value as a string.
    /// </summary>
    /// <param name="gridControl">The control containing the value.</param>
    /// <param name="dependencies">The dependencies of the property.</param>
    /// <returns>
    /// The grid cell value to be deployed.
    /// </returns>
    /// <remarks>
    /// Note that
    /// </remarks>
    [Obsolete("Use the overload accepting IContextCache instead. This overload will be removed in a future version.")]
    string? GetValue(GridValue.GridControl gridControl, ICollection<ArtifactDependency> dependencies)
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

    /// <summary>
    /// Allows you to modify the value of a control being deployed.
    /// </summary>
    /// <param name="gridControl">The control being deployed.</param>
    /// <remarks>
    /// Follows the pattern of the property value connectors (<see cref="IValueConnector" />).
    /// The SetValue method is used to modify the value of the <paramref name="gridControl" />.
    /// </remarks>
    [Obsolete("Use the overload accepting IContextCache instead. This overload will be removed in a future version.")]
    void SetValue(GridValue.GridControl gridControl)
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
