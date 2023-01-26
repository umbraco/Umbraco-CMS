using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Mapping.Dictionary;

public class DictionaryMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IDictionaryItem, DictionaryItemViewModel>((_, _) => new DictionaryItemViewModel(), Map);
        mapper.Define<IDictionaryTranslation, DictionaryItemTranslationModel>((_, _) => new DictionaryItemTranslationModel(), Map);
        mapper.Define<DictionaryItemUpdateModel, IDictionaryItem>((_, _) => new DictionaryItem(string.Empty), Map);
        mapper.Define<DictionaryItemCreateModel, IDictionaryItem>((_, _) => new DictionaryItem(string.Empty), Map);
        mapper.Define<IDictionaryItem, DictionaryOverviewViewModel>((_, _) => new DictionaryOverviewViewModel(), Map);
    }

    // Umbraco.Code.MapAll -Translations
    private void Map(IDictionaryItem source, DictionaryItemViewModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.Name = source.ItemKey;
    }

    // Umbraco.Code.MapAll
    private void Map(IDictionaryTranslation source, DictionaryItemTranslationModel target, MapperContext context)
    {
        target.IsoCode = source.IsoCode;
        target.Translation = source.Value;
    }

    // Umbraco.Code.MapAll -Id -Key -CreateDate -UpdateDate -ParentId -Translations
    private void Map(DictionaryItemUpdateModel source, IDictionaryItem target, MapperContext context)
    {
        target.ItemKey = source.Name;
        target.DeleteDate = null;
    }

    // Umbraco.Code.MapAll -Id -Key -CreateDate -UpdateDate -Translations
    private void Map(DictionaryItemCreateModel source, IDictionaryItem target, MapperContext context)
    {
        target.ItemKey = source.Name;
        target.ParentId = source.ParentKey;
        target.DeleteDate = null;
    }

    // Umbraco.Code.MapAll -Level
    private void Map(IDictionaryItem source, DictionaryOverviewViewModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.Name = source.ItemKey;
        target.TranslatedIsoCodes = source
            .Translations
            .Where(translation => translation.Value.IsNullOrWhiteSpace() == false)
            .Select(translation => translation.IsoCode)
            .ToArray();
    }
}
