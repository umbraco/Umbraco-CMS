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
    private readonly ILocalizationService _localizationService;

    [Obsolete("Use the constructor with the CommonMapper")]
    public DictionaryMapDefinition(ILocalizationService localizationService) : this(localizationService, StaticServiceProvider.Instance.GetRequiredService<CommonMapper>())
    {
    }

    public DictionaryMapDefinition(ILocalizationService localizationService, CommonMapper commonMapper)
    {
        _localizationService = localizationService;
        _commonMapper = commonMapper;
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

    private static void GetParentId(Guid parentId, ILocalizationService localizationService, List<int> ids)
    {
        IDictionaryItem? dictionary = localizationService.GetDictionaryItemById(parentId);
        if (dictionary == null)
        {
            return;
        }

        ids.Add(dictionary.Id);

        if (dictionary.ParentId.HasValue)
        {
            GetParentId(dictionary.ParentId.Value, localizationService, ids);
        }
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

        // build up the path to make it possible to set active item in tree
        // TODO: check if there is a better way
        if (source.ParentId.HasValue)
        {
            var ids = new List<int> { -1 };
            var parentIds = new List<int>();
            GetParentId(source.ParentId.Value, _localizationService, parentIds);
            parentIds.Reverse();
            ids.AddRange(parentIds);
            ids.Add(source.Id);
            target.Path = string.Join(",", ids);
        }
        else
        {
            target.Path = "-1," + source.Id;
        }

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
