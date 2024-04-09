using Umbraco.Cms.Api.Management.Services.OperationStatus;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Services;

public interface IDictionaryItemImportService
{
    Task<Attempt<IDictionaryItem?, DictionaryImportOperationStatus>> ImportDictionaryItemFromUdtFileAsync(Guid temporaryFileKey, Guid? parentKey, Guid userKey);
}
