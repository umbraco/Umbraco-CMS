using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Mapping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

namespace Umbraco.Cms.ManagementApi.Mapping.Dictionary;

public class DictionaryViewModelMapDefinition : IMapDefinition
{
    private readonly ILocalizationService _localizationService;
    private readonly CommonMapper _commonMapper;
    private readonly IDictionaryService _dictionaryService;

    public DictionaryViewModelMapDefinition(ILocalizationService localizationService, CommonMapper commonMapper, IDictionaryService dictionaryService)
    {
        _localizationService = localizationService;
        _commonMapper = commonMapper;
        _dictionaryService = dictionaryService;
    }
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<DictionaryViewModel, IDictionaryItem>((source, context) => new DictionaryItem(string.Empty));
        mapper.Define<IDictionaryItem, DictionaryViewModel>((source, context) => new DictionaryViewModel());
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

    // Umbraco.Code.MapAll -Icon -Trashed -Alias
    private void Map(IDictionaryItem source, DictionaryViewModel target, MapperContext context)
    {
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.ItemKey;
        target.ParentId = source.ParentId ?? Guid.Empty;
        target.Udi = Udi.Create(Constants.UdiEntityType.DictionaryItem, source.Key);
        target.ContentApps.AddRange(_commonMapper.GetContentAppsForEntity(source));
        target.Path = _dictionaryService.CalculatePath(source.ParentId, source.Id);

        // add all languages and  the translations
        foreach (ILanguage lang in _localizationService.GetAllLanguages())
        {
            var langId = lang.Id;
            IDictionaryTranslation? translation = source.Translations?.FirstOrDefault(x => x.LanguageId == langId);

            target.Translations.Add(new DictionaryTranslationDisplay
            {
                IsoCode = lang.IsoCode,
                DisplayName = lang.CultureName,
                Translation = translation?.Value ?? string.Empty,
                LanguageId = lang.Id,
            });
        }

        target.CreateDate = source.CreateDate;
        target.NameIsDirty = false; // fixme
        target.UpdateDate = source.UpdateDate;
    }
}
