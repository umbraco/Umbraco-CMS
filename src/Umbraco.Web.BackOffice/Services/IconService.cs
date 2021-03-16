using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ganss.XSS;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Services
{
    public class IconService : IIconService
    {
        private readonly IOptions<GlobalSettings> _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHtmlSanitizer _htmlSanitizer;

        public IconService(
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            IHtmlSanitizer htmlSanitizer)
        {
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
            _htmlSanitizer = htmlSanitizer;
        }


        /// <inheritdoc />
        public IList<IconModel> GetAllIcons()
        {
            var directory = new DirectoryInfo(_hostingEnvironment.MapPathWebRoot($"{_globalSettings.Value.IconsPath}/"));
            var iconNames = directory.GetFiles("*.svg");

            return iconNames.OrderBy(f => f.Name)
                .Select(iconInfo => GetIcon(iconInfo)).WhereNotNull().ToList();

        }

        /// <inheritdoc />
        public IconModel GetIcon(string iconName)
        {
            return string.IsNullOrWhiteSpace(iconName)
                ? null
                : CreateIconModel(iconName.StripFileExtension(), _hostingEnvironment.MapPathWebRoot($"{_globalSettings.Value.IconsPath}/{iconName}.svg"));
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
