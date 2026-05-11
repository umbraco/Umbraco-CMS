using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods for creating instances of template presentation objects within the management API.
/// </summary>
public class TemplatePresentationFactory : ITemplatePresentationFactory
{
    private readonly ITemplateService _templateService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Factories.TemplatePresentationFactory"/> class.
    /// </summary>
    /// <param name="templateService">An instance of <see cref="ITemplateService"/> used for template operations.</param>
    /// <param name="mapper">An instance of <see cref="IUmbracoMapper"/> used for mapping objects.</param>
    public TemplatePresentationFactory(
        ITemplateService templateService,
        IUmbracoMapper mapper)
    {
        _templateService = templateService;
        _mapper = mapper;
    }

    /// <summary>
    /// Asynchronously creates a <see cref="Umbraco.Cms.Api.Management.Models.TemplateResponseModel" /> from the specified <see cref="Umbraco.Cms.Core.Models.ITemplate" />.
    /// If the template has a master template, its reference will be included in the response model.
    /// </summary>
    /// <param name="template">The template from which to create the response model.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the created <see cref="Umbraco.Cms.Api.Management.Models.TemplateResponseModel" />.
    /// </returns>
    public async Task<TemplateResponseModel> CreateTemplateResponseModelAsync(ITemplate template)
    {
        TemplateResponseModel responseModel = new()
        {
            Id = template.Key,
            Name = template.Name ?? string.Empty,
            Alias = template.Alias,
            Content = template.Content
        };

        if (template.MasterTemplateAlias is not null)
        {
            ITemplate? parentTemplate = await _templateService.GetAsync(template.MasterTemplateAlias);
            responseModel.MasterTemplate = ReferenceByIdModel.ReferenceOrNull(parentTemplate?.Key);
        }

        return responseModel;
    }
}
