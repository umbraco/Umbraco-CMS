using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

namespace Umbraco.New.Cms.Core.Factories;

public interface IDictionaryFactory
{
    IDictionaryItem CreateDictionary(DictionaryViewModel dictionaryViewModel);
}
