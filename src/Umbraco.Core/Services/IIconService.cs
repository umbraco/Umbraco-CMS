using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IIconService
    {
        /// <summary>
        /// Gets an IconModel containing the icon name and SvgString according to an icon name found at the global icons path
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns><see cref="IconModel"/></returns>
        IconModel GetIcon(string iconName);

        /// <summary>
        /// Gets a list of all svg icons found at at the global icons path.
        /// </summary>
        /// <returns>A list of <see cref="IconModel"/></returns>
        IList<IconModel> GetAllIcons();
    }
}
