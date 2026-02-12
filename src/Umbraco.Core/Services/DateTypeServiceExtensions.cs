using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IDataTypeService"/>.
/// </summary>
public static class DateTypeServiceExtensions
{
    /// <summary>
    /// Determines whether the data type is configured to ignore user start nodes.
    /// </summary>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="key">The data type key.</param>
    /// <returns><c>true</c> if the data type ignores user start nodes; otherwise, <c>false</c>.</returns>
    public static bool IsDataTypeIgnoringUserStartNodes(this IDataTypeService dataTypeService, Guid key)
        => dataTypeService.IsDataTypeIgnoringUserStartNodesAsync(key).GetAwaiter().GetResult();

    /// <summary>
    /// Determines asynchronously whether the data type is configured to ignore user start nodes.
    /// </summary>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="key">The data type key.</param>
    /// <returns><c>true</c> if the data type ignores user start nodes; otherwise, <c>false</c>.</returns>
    public static async Task<bool> IsDataTypeIgnoringUserStartNodesAsync(this IDataTypeService dataTypeService, Guid key)
    {
        IDataType? dataType = await dataTypeService.GetAsync(key);
        return dataType is { ConfigurationObject: IIgnoreUserStartNodesConfig { IgnoreUserStartNodes: true } };
    }
}
