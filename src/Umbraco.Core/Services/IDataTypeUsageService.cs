using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IDataTypeUsageService
{

    [Obsolete("Please use HasSavedValuesAsync. Scheduled for removable in Umbraco 15.")]
    bool HasSavedValues(int dataTypeId);

    /// <summary>
    /// Checks if there are any saved property values using a given data type.
    /// </summary>
    /// <param name="dataTypeKey">The key of the data type to check.</param>
    /// <returns>An attempt with status and result if there are any property values using the data type, otherwise false.</returns>
    Task<Attempt<bool, DataTypeOperationStatus>> HasSavedValuesAsync(Guid dataTypeKey);
}
