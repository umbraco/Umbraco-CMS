using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Defines methods that can convert a grid cell value to / from an environment-agnostic string.
/// </summary>
/// <remarks>
///     Grid cell values may contain values such as content identifiers, that would be local
///     to one environment, and need to be converted in order to be deployed.
/// </remarks>
public interface IGridCellValueConnector
{
    /// <summary>
    ///     Gets a value indicating whether the connector supports a specified grid editor view.
    /// </summary>
    /// <param name="view">
    ///     The grid editor view. It needs to be the view instead of the alias as the view is really what
    ///     identifies what kind of connector should be used. Alias can be anything and you can have multiple different aliases
    ///     using the same kind of view.
    /// </param>
    /// <remarks>A value indicating whether the connector supports the grid editor view.</remarks>
    /// <remarks>Note that <paramref name="view" /> can be string.Empty to indicate the "default" connector.</remarks>
    bool IsConnector(string view);

    /// <summary>
    ///     Gets the value to be deployed from the control value as a string.
    /// </summary>
    /// <param name="gridControl">The control containing the value.</param>
    /// <param name="dependencies">The dependencies of the property.</param>
    /// <returns>The grid cell value to be deployed.</returns>
    /// <remarks>Note that </remarks>
    string? GetValue(GridValue.GridControl gridControl, ICollection<ArtifactDependency> dependencies);

    /// <summary>
    ///     Allows you to modify the value of a control being deployed.
    /// </summary>
    /// <param name="gridControl">The control being deployed.</param>
    /// <remarks>
    ///     Follows the pattern of the property value connectors (<see cref="IValueConnector" />). The SetValue method is
    ///     used to modify the value of the <paramref name="gridControl" />.
    /// </remarks>
    void SetValue(GridValue.GridControl gridControl);
}
