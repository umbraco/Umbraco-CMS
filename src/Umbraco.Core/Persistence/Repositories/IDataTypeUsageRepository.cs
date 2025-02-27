namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IDataTypeUsageRepository
{
    [Obsolete("Please use HasSavedValuesAsync. Scheduled for removable in Umbraco 15.")]
    bool HasSavedValues(int dataTypeId);
    Task<bool> HasSavedValuesAsync(Guid dataTypeKey);
}
