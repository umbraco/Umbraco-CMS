using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.ManagementApi.Services;

public interface ILoadDictionaryItemService
{
    IDictionaryItem Load(string filePath, int? parentId);
}
