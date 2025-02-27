using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.Templates;

/// <summary>
///     Methods used to render umbraco components as HTML in templates
/// </summary>
/// <remarks>
///     Used by UmbracoHelper
/// </remarks>
public class UmbracoComponentRenderer : IUmbracoComponentRenderer
{
    private readonly ITemplateRenderer _templateRenderer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoComponentRenderer" /> class.
    /// </summary>
    public UmbracoComponentRenderer(ITemplateRenderer templateRenderer)
        => _templateRenderer = templateRenderer ?? throw new ArgumentNullException(nameof(templateRenderer));

    /// <inheritdoc />
    public async Task<IHtmlEncodedString> RenderTemplateAsync(int contentId, int? altTemplateId = null)
    {
        using (var sw = new StringWriter())
        {
            try
            {
                await _templateRenderer.RenderAsync(contentId, altTemplateId, sw);
            }
            catch (Exception ex)
            {
                sw.Write("<!-- Error rendering template with id {0}: '{1}' -->", contentId, ex);
            }

            return new HtmlEncodedString(sw.ToString());
        }
    }
}
