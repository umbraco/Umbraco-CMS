using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory for creating template presentation models.
/// </summary>
public interface ITemplatePresentationFactory
{
    /// <summary>
    /// Creates a <see cref="TemplateResponseModel"/> asynchronously from the given <see cref="ITemplate"/>.
    /// </summary>
    /// <param name="template">The template to create the response model from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="TemplateResponseModel"/>.</returns>
    Task<TemplateResponseModel> CreateTemplateResponseModelAsync(ITemplate template);
}
