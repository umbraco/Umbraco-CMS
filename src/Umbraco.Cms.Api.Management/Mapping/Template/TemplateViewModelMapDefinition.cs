using Umbraco.Cms.Api.Management.ViewModels.Template;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Api.Management.Mapping.Template;

public class TemplateViewModelMapDefinition : IMapDefinition
{
    private readonly IShortStringHelper _shortStringHelper;

    public TemplateViewModelMapDefinition(IShortStringHelper shortStringHelper)
        => _shortStringHelper = shortStringHelper;

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
