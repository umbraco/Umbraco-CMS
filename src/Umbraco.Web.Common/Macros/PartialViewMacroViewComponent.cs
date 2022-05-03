using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Macros;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Web.Common.Macros;

/// <summary>
///     Controller to render macro content for Partial View Macros
/// </summary>
// [MergeParentContextViewData] // TODO is this requeired now it is a ViewComponent?
[HideFromTypeFinder] // explicitly used: do *not* find and register it!
internal class PartialViewMacroViewComponent : ViewComponent
{
    private readonly IPublishedContent _content;
    private readonly MacroModel _macro;

    public PartialViewMacroViewComponent(
        MacroModel macro,
        IPublishedContent content,
        ViewComponentContext viewComponentContext)
    {
        _macro = macro;
        _content = content;

        // This must be set before Invoke is called else the call to View will end up
        // using an empty ViewData instance because this hasn't been set yet.
        ViewComponentContext = viewComponentContext;
    }

    public IViewComponentResult Invoke()
    {
        var model = new PartialViewMacroModel(
            _content,
            _macro.Id,
            _macro.Alias,
            _macro.Name,
            _macro.Properties.ToDictionary(x => x.Key, x => (object?)x.Value));

        ViewViewComponentResult result = View(_macro.MacroSource, model);

        return result;
    }
}
