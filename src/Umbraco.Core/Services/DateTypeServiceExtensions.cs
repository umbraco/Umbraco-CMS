using System;

namespace Umbraco.Core.Services
{
    public static class DateTypeServiceExtensions
    {
        public static bool IsDataTypeIgnoringUserStartNodes(this IDataTypeService dataTypeService, Guid key)
        {
            var dataType = dataTypeService.GetDataTypeDefinitionById(key);

            if (dataType != null)
            {
                var preValues = dataTypeService.GetPreValuesCollectionByDataTypeId(dataType.Id);
                if (preValues.PreValuesAsDictionary.TryGetValue(
                    Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes, out var preValue))
                    return preValue.Value.InvariantEquals("1");
            }

            return false;
        }
    }
}
