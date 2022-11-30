namespace Umbraco.Cms.Core.Services;

public interface IDataTypeUsageService
{
    /// <summary>
    /// Checks if there are any saved property values using a given data type.
    /// </summary>
    /// <param name="dataTypeId">The ID of the data type to check.</param>
    /// <returns>True if there are any property values using the data type, otherwise false.</returns>
    bool HasSavedValues(int dataTypeId);
}
