using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;
using Stylesheet = Umbraco.Cms.Core.Models.ContentEditing.Stylesheet;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     The API controller used for retrieving available stylesheets
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class StylesheetController : UmbracoAuthorizedJsonController
{
    private readonly IFileService _fileService;

    public StylesheetController(IFileService fileService) => _fileService = fileService;

    public IEnumerable<Stylesheet> GetAll() =>
        _fileService.GetStylesheets()
            .Select(x =>
                new Stylesheet { Name = x.Alias, Path = x.VirtualPath });

    public IEnumerable<StylesheetRule> GetRulesByName(string name)
    {
        IStylesheet? css = _fileService.GetStylesheet(name.EnsureEndsWith(".css"));
        if (css is null || css.Properties is null)
        {
            return Enumerable.Empty<StylesheetRule>();
        }

        return css.Properties.Select(x => new StylesheetRule { Name = x.Name, Selector = x.Alias });
    }
}
