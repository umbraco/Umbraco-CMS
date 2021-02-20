using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ganss.XSS;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.Services
{
    public class IconService : IIconService
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly IHtmlSanitizer _htmlSanitizer;

        public IconService(IGlobalSettings globalSettings,IHtmlSanitizer htmlSanitizer)
        {
            _globalSettings = globalSettings;
            _htmlSanitizer = htmlSanitizer;
        }


        /// <inheritdoc />
        public IList<IconModel> GetAllIcons()
        {
            var directory = new DirectoryInfo(IOHelper.MapPath($"{_globalSettings.IconsPath}/"));
            var iconNames = directory.GetFiles("*.svg");

            return iconNames.OrderBy(f => f.Name)
                .Select(iconInfo => GetIcon(iconInfo)).WhereNotNull().ToList();

        }

        /// <inheritdoc />
        public IconModel GetIcon(string iconName)
        {
            return string.IsNullOrWhiteSpace(iconName)
                ? null
                : CreateIconModel(iconName.StripFileExtension(), IOHelper.MapPath($"{_globalSettings.IconsPath}/{iconName}.svg"));
        }

        /// <summary>
        /// Gets an IconModel using values from a FileInfo model
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        private IconModel GetIcon(FileInfo fileInfo)
        {
            return fileInfo == null || string.IsNullOrWhiteSpace(fileInfo.Name)
                ? null
                : CreateIconModel(fileInfo.Name.StripFileExtension(), fileInfo.FullName);
        }

        /// <summary>
        /// Gets an IconModel containing the icon name and SvgString
        /// </summary>
        /// <param name="iconName"></param>
        /// <param name="iconPath"></param>
        /// <returns></returns>
        private IconModel CreateIconModel(string iconName, string iconPath)
        {
            try
            {
                var svgContent = System.IO.File.ReadAllText(iconPath);
                var sanitizedString = _htmlSanitizer.Sanitize(svgContent);

                var svg = new IconModel
                {
                    Name = iconName,
                    SvgString = sanitizedString
                };

                return svg;
            }
            catch
            {
                return null;
            }
        }
    }
}
