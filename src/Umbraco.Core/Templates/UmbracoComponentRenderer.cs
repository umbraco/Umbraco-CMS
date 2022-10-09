using System.Net;
using Umbraco.Cms.Core.Macros;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Templates;

/// <summary>
///     Methods used to render umbraco components as HTML in templates
/// </summary>
/// <remarks>
///     Used by UmbracoHelper
/// </remarks>
public class UmbracoComponentRenderer : IUmbracoComponentRenderer
{
    private readonly IMacroRenderer _macroRenderer;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoComponentRenderer" /> class.
    /// </summary>
    public UmbracoComponentRenderer(IUmbracoContextAccessor umbracoContextAccessor, IMacroRenderer macroRenderer, ITemplateRenderer templateRenderer)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _macroRenderer = macroRenderer;
        _templateRenderer = templateRenderer ?? throw new ArgumentNullException(nameof(templateRenderer));
    }

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

    /// <inheritdoc />
    public async Task<IHtmlEncodedString> RenderMacroAsync(int contentId, string alias) =>
        await RenderMacroAsync(contentId, alias, new { });

    /// <inheritdoc />
    public async Task<IHtmlEncodedString> RenderMacroAsync(int contentId, string alias, object parameters) =>
        await RenderMacroAsync(contentId, alias, parameters.ToDictionary<object>());

    /// <inheritdoc />
    public async Task<IHtmlEncodedString> RenderMacroAsync(int contentId, string alias, IDictionary<string, object>? parameters)
    {
        if (contentId == default)
        {
            throw new ArgumentException("Invalid content id " + contentId);
        }

        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
        IPublishedContent? content = umbracoContext.Content?.GetById(contentId);

        if (content == null)
        {
            throw new InvalidOperationException("Cannot render a macro, no content found by id " + contentId);
        }

        return await RenderMacroAsync(content, alias, parameters);
    }

    /// <inheritdoc />
    public async Task<IHtmlEncodedString> RenderMacroForContent(IPublishedContent content, string alias, IDictionary<string, object>? parameters)
    {
        if (content == null)
        {
            throw new InvalidOperationException("Cannot render a macro, IPublishedContent is null");
        }

        return await RenderMacroAsync(content, alias, parameters);
    }

    /// <summary>
    ///     Renders the macro with the specified alias, passing in the specified parameters.
    /// </summary>
    private async Task<IHtmlEncodedString> RenderMacroAsync(IPublishedContent content, string alias, IDictionary<string, object>? parameters)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        // TODO: We are doing at ToLower here because for some insane reason the UpdateMacroModel method looks for a lower case match. the whole macro concept needs to be rewritten.
        // NOTE: the value could have HTML encoded values, so we need to deal with that
        var macroProps = parameters?.ToDictionary(
            x => x.Key.ToLowerInvariant(),
            i => i.Value is string ? WebUtility.HtmlDecode(i.Value.ToString()) : i.Value);

        var html = (await _macroRenderer.RenderAsync(alias, content, macroProps)).Text;

        return new HtmlEncodedString(html!);
    }
}
