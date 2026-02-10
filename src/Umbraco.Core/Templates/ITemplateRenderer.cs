namespace Umbraco.Cms.Core.Templates;

/// <summary>
///     This is used purely for the RenderTemplate functionality in Umbraco
/// </summary>
public interface ITemplateRenderer
{
    /// <summary>
    ///     Renders a template for the specified page asynchronously.
    /// </summary>
    /// <param name="pageId">The identifier of the page to render.</param>
    /// <param name="altTemplateId">An optional alternative template identifier. If not specified, uses the template assigned to the page.</param>
    /// <param name="writer">The <see cref="StringWriter"/> to write the rendered output to.</param>
    /// <returns>A task that represents the asynchronous render operation.</returns>
    Task RenderAsync(int pageId, int? altTemplateId, StringWriter writer);
}
