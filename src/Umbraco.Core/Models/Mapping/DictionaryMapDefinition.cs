using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.Models.Mapping;

/// <inheritdoc />
/// <summary>
///     The dictionary model mapper.
/// </summary>
public class DictionaryMapDefinition : IMapDefinition
{
    private readonly CommonMapper? _commonMapper;
    private readonly IDictionaryService _dictionaryService;
    private readonly ILocalizationService _localizationService;

    [Obsolete("Use the constructor with the CommonMapper")]
    public DictionaryMapDefinition(ILocalizationService localizationService)
        : this(
        localizationService,
        StaticServiceProvider.Instance.GetRequiredService<CommonMapper>(),
        StaticServiceProvider.Instance.GetRequiredService<IDictionaryService>())
    {
    }

    [Obsolete("Use the constructor with the CommonMapper, and IDictionaryService")]
    public DictionaryMapDefinition(ILocalizationService localizationService, CommonMapper commonMapper)
        : this(
        localizationService,
        commonMapper,
        StaticServiceProvider.Instance.GetRequiredService<IDictionaryService>())
    {
    }

    public DictionaryMapDefinition(ILocalizationService localizationService, CommonMapper commonMapper, IDictionaryService dictionaryService)
    {
        _localizationService = localizationService;
        _commonMapper = commonMapper;
        _dictionaryService = dictionaryService;
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IDictionaryItem, EntityBasic>((source, context) => new EntityBasic(), Map);
        mapper.Define<IDictionaryItem, DictionaryDisplay>((source, context) => new DictionaryDisplay(), Map);
        mapper.Define<IDictionaryItem, DictionaryOverviewDisplay>(
            (source, context) => new DictionaryOverviewDisplay(),
            Map);
    }

    // Umbraco.Code.MapAll -ParentId -Path -Trashed -Udi -Icon
    private static void Map(IDictionaryItem source, EntityBasic target, MapperContext context)
    {
        target.Alias = source.ItemKey;
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.ItemKey;
    }

    // Umbraco.Code.MapAll -Icon -Trashed -Alias
    private void Map(IDictionaryItem source, DictionaryDisplay target, MapperContext context)
    {
        target.Id = source.Id;
        target.Key = source.Key;
        target.Name = source.ItemKey;
        target.ParentId = source.ParentId ?? Guid.Empty;
        target.Udi = Udi.Create(Constants.UdiEntityType.DictionaryItem, source.Key);
        if (_commonMapper != null)
        {
            target.ContentApps.AddRange(_commonMapper.GetContentAppsForEntity(source));
        }

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
    }

    // Umbraco.Code.MapAll -Level -Translations
    private void Map(IDictionaryItem source, DictionaryOverviewDisplay target, MapperContext context)
    {
        target.Id = source.Id;
        target.Name = source.ItemKey;

        // add all languages and  the translations
        foreach (ILanguage lang in _localizationService.GetAllLanguages())
        {
            var langId = lang.Id;
            IDictionaryTranslation? translation = source.Translations?.FirstOrDefault(x => x.LanguageId == langId);

            target.Translations.Add(
                new DictionaryOverviewTranslationDisplay
                {
                    DisplayName = lang.CultureName,
                    HasTranslation = translation != null && string.IsNullOrEmpty(translation.Value) == false,
                });
        }
    }
}
