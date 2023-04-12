using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Services;

public interface ILoadDictionaryItemService
{
    IDictionaryItem Load(string filePath, int? parentId);
}
