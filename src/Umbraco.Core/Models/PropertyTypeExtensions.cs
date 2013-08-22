using Umbraco.Core.Services;
using umbraco.interfaces;

namespace Umbraco.Core.Models
{
    public static class PropertyTypeExtensions
    {
        /// <summary>
        /// Resolves the IDataType for a PropertyType.
        /// </summary>
        /// <param name="propertyType">PropertyType that references a DataType</param>
        /// <param name="propertyId">Id of the Property which references this DataType through its PropertyType</param>
        /// <param name="dataTypeService"></param>
        /// <returns><see cref="IDataType"/></returns>
        /// <remarks>
        /// This extension method is left internal because we don't want to take
        /// a hard dependency on the IDataType, as it will eventually go away and
        /// be replaced by PropertyEditors. It is however needed to generate xml
        /// for a property/propertytype when publishing.
        /// </remarks>
        internal static IDataType DataType(this PropertyType propertyType, int propertyId, IDataTypeService dataTypeService)
        {
            Mandate.ParameterNotNull(propertyType, "propertyType");
            var dataType = dataTypeService.GetDataTypeById(propertyType.DataTypeId);
            if (dataType == null)
            {
                return null;
            }
            dataType.DataTypeDefinitionId = propertyType.DataTypeDefinitionId;
            dataType.Data.PropertyId = propertyId;
            return dataType;
        }
    }
}