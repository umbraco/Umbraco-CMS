using umbraco.interfaces;

namespace Umbraco.Core.Models
{
    public static class PropertyTypeExtensions
    {
        /// <summary>
        /// Resolves the IDataType for a PropertyType.
        /// </summary>
        /// <param name="propertyType">PropertyType that references a DataType</param>
        /// <returns><see cref="IDataType"/></returns>
        /// <remarks>
        /// This extension method is left internal because we don't want to take
        /// a hard dependency on the IDataType, as it will eventually go away and
        /// be replaced by PropertyEditors. It is however needed to generate xml
        /// for a property/propertytype when publishing.
        /// </remarks>
        internal static IDataType DataType(this PropertyType propertyType)
        {
            Mandate.ParameterNotNull(propertyType, "propertyType");
            var dataType = DataTypesResolver.Current.GetById(propertyType.DataTypeControlId);
            dataType.DataTypeDefinitionId = propertyType.DataTypeId;
            return dataType;
        }
    }
}