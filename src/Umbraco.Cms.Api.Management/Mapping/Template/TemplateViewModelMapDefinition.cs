﻿using Umbraco.Cms.Api.Management.ViewModels.Template;
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
        mapper.Define<ITemplate, TemplateViewModel>((_, _) => new TemplateViewModel(), Map);
        mapper.Define<TemplateUpdateModel, ITemplate>((source, _) => new Core.Models.Template(_shortStringHelper, source.Name, source.Alias), Map);
    }

    // Umbraco.Code.MapAll
    private void Map(ITemplate source, TemplateViewModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.Name = source.Name ?? string.Empty;
        target.Alias = source.Alias;
        target.Content = source.Content;
    }

    // Umbraco.Code.MapAll -Id -Key -CreateDate -UpdateDate -DeleteDate
    // Umbraco.Code.MapAll -Path -VirtualPath -MasterTemplateId -IsMasterTemplate
    private void Map(TemplateUpdateModel source, ITemplate target, MapperContext context)
    {
        target.Name = source.Name;
        target.Alias = source.Alias;
        target.Content = source.Content;
    }
}
