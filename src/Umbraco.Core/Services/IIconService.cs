using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IIconService
    {
        /// <summary>
        /// Gets the svg string for the icon name found at the global icons path
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns></returns>
        IconModel GetIcon(string iconName);

        /// <summary>
        /// Gets a list of all svg icons found at at the global icons path.
        /// </summary>
        /// <returns>A list of <see cref="IconModel"/></returns>
        [Obsolete("This method should not be used - use GetIcons instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IList<IconModel> GetAllIcons();

        /// <summary>
        /// Gets a list of all svg icons found at at the global icons path.
        /// </summary>
        /// <returns></returns>
        IReadOnlyDictionary<string, string> GetIcons();
    }
}
