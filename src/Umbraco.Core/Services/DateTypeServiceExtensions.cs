using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

public static class DateTypeServiceExtensions
{
    public static bool IsDataTypeIgnoringUserStartNodes(this IDataTypeService dataTypeService, Guid key)
    {
        if (DataTypeExtensions.IsBuildInDataType(key))
        {
            return false; // built in ones can never be ignoring start nodes
        }

        IDataType? dataType = dataTypeService.GetAsync(key).GetAwaiter().GetResult();

        if (dataType != null && dataType.ConfigurationObject is IIgnoreUserStartNodesConfig ignoreStartNodesConfig)
        {
            return ignoreStartNodesConfig.IgnoreUserStartNodes;
        }

        return false;
    }
}
