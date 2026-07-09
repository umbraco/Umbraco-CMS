using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Api.Management.Mapping.Template;

/// <summary>
/// Provides the mapping configuration between the Template domain model and its corresponding view model.
/// </summary>
public class TemplateViewModelMapDefinition : IMapDefinition
{
    private readonly IShortStringHelper _shortStringHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateViewModelMapDefinition"/> class.
    /// </summary>
    /// <param name="shortStringHelper">An instance of <see cref="IShortStringHelper"/> used to assist with short string operations.</param>
    public TemplateViewModelMapDefinition(IShortStringHelper shortStringHelper)
        => _shortStringHelper = shortStringHelper;

    /// <summary>
    /// Configures object-object mappings for template view models, specifically mapping <see cref="UpdateTemplateRequestModel"/> to <see cref="ITemplate"/>.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used to define the mappings.</param>
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<UpdateTemplateRequestModel, ITemplate>((source, _) => new Core.Models.Template(_shortStringHelper, source.Name, source.Alias), Map);
    }

    // Umbraco.Code.MapAll -Id -Key -CreateDate -UpdateDate -DeleteDate
    // Umbraco.Code.MapAll -Path -VirtualPath -MasterTemplateId -IsMasterTemplate
    private void Map(UpdateTemplateRequestModel source, ITemplate target, MapperContext context)
    {
        target.Name = source.Name;
        target.Alias = source.Alias;
        target.Content = source.Content;
    }
}
