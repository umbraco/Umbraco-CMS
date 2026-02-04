namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for querying data type usage information.
/// </summary>
public interface IDataTypeUsageRepository
{
    /// <summary>
    ///     Determines whether there are any saved property values using the specified data type.
    /// </summary>
    /// <param name="dataTypeKey">The unique key of the data type.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <c>true</c> if there are saved values;
    ///     otherwise, <c>false</c>.
    /// </returns>
    Task<bool> HasSavedValuesAsync(Guid dataTypeKey);
}
