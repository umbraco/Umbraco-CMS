﻿using Umbraco.Cms.Api.Management.Mapping.ContentType;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.MemberType;

public class MemberTypeMapDefinition : ContentTypeMapDefinition<IMemberType, MemberTypePropertyTypeResponseModel, MemberTypePropertyTypeContainerResponseModel>, IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IMemberType, MemberTypeResponseModel>((_, _) => new MemberTypeResponseModel(), Map);
        mapper.Define<IMemberType, MemberTypeReferenceResponseModel>((_, _) => new MemberTypeReferenceResponseModel(), Map);
        mapper.Define<ISimpleContentType, MemberTypeReferenceResponseModel>((_, _) => new MemberTypeReferenceResponseModel(), Map);
    }

    // Umbraco.Code.MapAll -Collection
    private void Map(IMemberType source, MemberTypeResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Alias = source.Alias;
        target.Name = source.Name ?? string.Empty;
        target.Description = source.Description;
        target.Icon = source.Icon ?? string.Empty;
        target.AllowedAsRoot = source.AllowedAsRoot;
        target.VariesByCulture = source.VariesByCulture();
        target.VariesBySegment = source.VariesBySegment();
        target.IsElement = source.IsElement;
        target.Containers = MapPropertyTypeContainers(source);
        target.Properties = MapPropertyTypes(source);

        target.Compositions = source.ContentTypeComposition.Select(contentTypeComposition => new MemberTypeComposition
        {
            MemberType = new ReferenceByIdModel(contentTypeComposition.Key),
            CompositionType = CalculateCompositionType(source, contentTypeComposition)
        }).ToArray();
    }

    // Umbraco.Code.MapAll -Collection
    private void Map(IMemberType source, MemberTypeReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Icon = source.Icon ?? string.Empty;
    }

    // Umbraco.Code.MapAll -Collection
    private void Map(ISimpleContentType source, MemberTypeReferenceResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Icon = source.Icon ?? string.Empty;
    }
}
