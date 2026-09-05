using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for <see cref="IPropertyType" />.
/// </summary>
public static class PropertyTypeExtensions
{
    /// <summary>
    ///     Resolves the data type referenced by the given property type.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="idKeyMap">The cached id-to-key map used to resolve int data type IDs to GUID keys.</param>
    /// <returns>
    ///     The resolved <see cref="IDataType" />, or <c>null</c> if the id-to-key lookup fails or the data type
    ///     cannot be loaded.
    /// </returns>
    /// <remarks>
    ///     Prefers <see cref="IPropertyType.DataTypeKey" /> directly when set; falls back to mapping the int
    ///     <see cref="IPropertyType.DataTypeId" /> via <see cref="IIdKeyMap" />.
    /// </remarks>
    public static IDataType? GetDataType(this IPropertyType propertyType, IDataTypeService dataTypeService, IIdKeyMap idKeyMap)
    {
        Guid dataTypeKey = propertyType.DataTypeKey;
        if (dataTypeKey == Guid.Empty)
        {
            Attempt<Guid> keyAttempt = idKeyMap.GetKeyForId(propertyType.DataTypeId, UmbracoObjectTypes.DataType);
            if (keyAttempt.Success is false)
            {
                return null;
            }

            dataTypeKey = keyAttempt.Result;
        }

        return dataTypeService.GetAsync(dataTypeKey).GetAwaiter().GetResult();
    }
}
