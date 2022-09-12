using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;
using Umbraco.New.Cms.Core.Factories;

namespace Umbraco.Cms.ManagementApi.Factories;

public class DictionaryFactory : IDictionaryFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ILocalizationService _localizationService;

    public DictionaryFactory(IUmbracoMapper umbracoMapper, ILocalizationService localizationService)
    {
        _umbracoMapper = umbracoMapper;
        _localizationService = localizationService;
    }

    public IDictionaryItem CreateDictionary(DictionaryViewModel dictionaryViewModel)
    {
        IDictionaryItem dictionaryToSave = _umbracoMapper.Map<IDictionaryItem>(dictionaryViewModel)!;
        IDictionaryItem? dic = _localizationService.GetDictionaryItemById(dictionaryViewModel.Key);
        dictionaryToSave.Id = dic!.Id;
        return dictionaryToSave;
    }
}
