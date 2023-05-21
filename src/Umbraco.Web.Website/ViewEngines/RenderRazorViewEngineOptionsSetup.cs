using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Web.Website.ViewEngines;

/// <summary>
///     Configure view engine locations for front-end rendering
/// </summary>
public class RenderRazorViewEngineOptionsSetup : IConfigureOptions<RazorViewEngineOptions>
{
    /// <inheritdoc />
    public void Configure(RazorViewEngineOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.ViewLocationExpanders.Add(new ViewLocationExpander());
    }

    /// <summary>
    ///     Expands the default view locations
    /// </summary>
    private class ViewLocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(
            ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            string[] umbViewLocations =
            {
                "/Views/{0}.cshtml", "/Views/Shared/{0}.cshtml", "/Views/Partials/{0}.cshtml",
                "/Views/MacroPartials/{0}.cshtml",
            };

            viewLocations = umbViewLocations.Concat(viewLocations);

            return viewLocations;
        }

        // not a dynamic expander
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }
    }
}
