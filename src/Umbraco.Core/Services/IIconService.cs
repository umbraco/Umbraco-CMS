using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IIconService
{
    /// <summary>
    ///     Gets the svg string for the icon name found at the global icons path
    /// </summary>
    /// <param name="iconName"></param>
    /// <returns></returns>
    IconModel? GetIcon(string iconName);

    /// <summary>
    ///     Gets a list of all svg icons found at at the global icons path.
    /// </summary>
    /// <returns></returns>
    IReadOnlyDictionary<string, string>? GetIcons();
}
