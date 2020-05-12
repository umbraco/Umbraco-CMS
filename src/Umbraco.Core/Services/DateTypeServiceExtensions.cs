using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    internal static class DateTypeServiceExtensions
    {
        public static bool IsDataTypeIgnoringUserStartNodes(this IDataTypeService dataTypeService, Guid key)
        {
            if (DataTypeExtensions.IsBuildInDataType(key)) return false; //built in ones can never be ignoring start nodes

            var dataType = dataTypeService.GetDataTypeDefinitionById(key);

            if (dataType != null)
            {
                var preValues = dataTypeService.GetPreValuesCollectionByDataTypeId(dataType.Id);
                if (preValues.FormatAsDictionary().TryGetValue(
                    Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes, out var preValue))
                    return preValue.Value.InvariantEquals("1");
            }

            return false;
        }
    }
}
