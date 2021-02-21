using System.Collections.Generic;

namespace Umbraco.Core.Services
{
    public interface IIconService
    {
        /// <summary>
        /// Gets the svg string for the icon name found at the global icons path
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns></returns>
        string GetIcon(string iconName);

        /// <summary>
        /// Gets a list of all svg icons found at at the global icons path.
        /// </summary>
        /// <returns></returns>
        IReadOnlyDictionary<string, string> GetAllIcons();
    }
}
