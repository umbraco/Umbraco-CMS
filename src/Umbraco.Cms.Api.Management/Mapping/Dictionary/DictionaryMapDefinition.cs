using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Dictionary;

public class DictionaryMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IDictionaryItem, DictionaryItemResponseModel>((_, _) => new DictionaryItemResponseModel(), Map);
        mapper.Define<IDictionaryTranslation, DictionaryItemTranslationModel>((_, _) => new DictionaryItemTranslationModel(), Map);
        mapper.Define<UpdateDictionaryItemRequestModel, IDictionaryItem>((_, _) => new DictionaryItem(string.Empty), Map);
        mapper.Define<CreateDictionaryItemRequestModel, IDictionaryItem>((_, _) => new DictionaryItem(string.Empty), Map);
        mapper.Define<IDictionaryItem, DictionaryOverviewResponseModel>((_, _) => new DictionaryOverviewResponseModel(), Map);
    }

    // Umbraco.Code.MapAll -Translations
    private void Map(IDictionaryItem source, DictionaryItemResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.ItemKey;
    }

    // Umbraco.Code.MapAll
    private void Map(IDictionaryTranslation source, DictionaryItemTranslationModel target, MapperContext context)
    {
        target.IsoCode = source.LanguageIsoCode;
        target.Translation = source.Value;
    }

    // Umbraco.Code.MapAll -Id -Key -CreateDate -UpdateDate -ParentId -Translations
    private void Map(UpdateDictionaryItemRequestModel source, IDictionaryItem target, MapperContext context)
    {
        target.ItemKey = source.Name;
        target.DeleteDate = null;
    }

    // Umbraco.Code.MapAll -Id -CreateDate -UpdateDate -Translations
    private void Map(CreateDictionaryItemRequestModel source, IDictionaryItem target, MapperContext context)
    {
        if (source.Id is not null)
        {
            target.Key = source.Id.Value;
        }

        target.ItemKey = source.Name;
        target.ParentId = source.Parent?.Id;
        target.DeleteDate = null;
    }

    // Umbraco.Code.MapAll
    private void Map(IDictionaryItem source, DictionaryOverviewResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.Name = source.ItemKey;
        target.Parent = ReferenceByIdModel.ReferenceOrNull(source.ParentId);
        target.TranslatedIsoCodes = source
            .Translations
            .Where(translation => translation.Value.IsNullOrWhiteSpace() == false)
            .Select(translation => translation.LanguageIsoCode)
            .ToArray();
    }
}
