using System.Collections.Generic;
using System.IO;
using Umbraco.Web.Models;

namespace Umbraco.Web.Services
{
    public interface IIconService
    {
        /// <summary>
        /// Gets an IconModel containing the icon name and SvgString according to an icon name found at the global icons path
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns></returns>
        IconModel GetIcon(string iconName);

        /// <summary>
        /// Gets an IconModel using values from a FileInfo model
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        IconModel GetIcon(FileInfo fileInfo);

        /// <summary>
        /// Gets a list of all svg icons found at at the global icons path.
        /// </summary>
        /// <returns></returns>
        List<IconModel> GetAllIcons();
    }
}
