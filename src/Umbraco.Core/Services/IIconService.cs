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
        /// <returns></returns>
        IconModel GetIcon(string iconName);

        /// <summary>
        /// Gets a list of all svg icons found at the global icons path.
        /// </summary>
        /// <returns></returns>
        IList<IconModel> GetAllIcons();

        /// <summary>
        /// Gets a paged list of svg icons found at the global icons path.
        /// </summary>
        /// <returns></returns>
        IList<IconModel> GetPagedIcons(int pageIndex, int pageSize, out long totalRecords, string filter = "");
    }
}
