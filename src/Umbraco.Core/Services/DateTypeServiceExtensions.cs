using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

public static class DateTypeServiceExtensions
{
    public static bool IsDataTypeIgnoringUserStartNodes(this IDataTypeService dataTypeService, Guid key)
        => dataTypeService.IsDataTypeIgnoringUserStartNodesAsync(key).GetAwaiter().GetResult();

    public static async Task<bool> IsDataTypeIgnoringUserStartNodesAsync(this IDataTypeService dataTypeService, Guid key)
    {
        IDataType? dataType = await dataTypeService.GetAsync(key);
        return dataType is { ConfigurationObject: IIgnoreUserStartNodesConfig { IgnoreUserStartNodes: true } };
    }
}
