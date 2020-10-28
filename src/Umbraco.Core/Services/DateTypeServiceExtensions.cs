using System;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Services
{
    [UmbracoVolatile]
    public static class DateTypeServiceExtensions
    {
        public static bool IsDataTypeIgnoringUserStartNodes(this IDataTypeService dataTypeService, Guid key)
        {
            if (DataTypeExtensions.IsBuildInDataType(key)) return false; //built in ones can never be ignoring start nodes

            var dataType = dataTypeService.GetDataType(key);

            if (dataType != null && dataType.Configuration is IIgnoreUserStartNodesConfig ignoreStartNodesConfig)
                return ignoreStartNodesConfig.IgnoreUserStartNodes;

            return false;
        }
    }
}
