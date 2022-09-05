using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

namespace Umbraco.Cms.ManagementApi.Mapping.Dictionary;

public class DictionaryViewModelMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<DictionaryViewModel, IDictionaryItem>((source, context) => new DictionaryItem(string.Empty));
        mapper.Define<DictionaryTranslationDisplay, IDictionaryTranslation>((source, context) => new DictionaryTranslation(source.LanguageId, string.Empty));
    }

    // Umbraco.Code.MapAll
    private void Map(DictionaryViewModel source, IDictionaryItem target, MapperContext context)
    {
        target.CreateDate = source.CreateDate;
        target.Id = (int)source.Id!;
        target.ItemKey = source.Name!;
        target.Key = source.Key;
        target.ParentId = source.ParentId;
        target.Translations = context.MapEnumerable<DictionaryTranslationDisplay, IDictionaryTranslation>(source.Translations);
        target.UpdateDate = source.UpdateDate;
        target.DeleteDate = null;

    }

    // Umbraco.Code.MapAll -CreateDate -DeleteDate -Id -Key -UpdateDate -Language
    private void Map(DictionaryTranslationDisplay source, IDictionaryTranslation target, MapperContext context)
    {
        target.Value = source.Translation; // fixme
    }
}
