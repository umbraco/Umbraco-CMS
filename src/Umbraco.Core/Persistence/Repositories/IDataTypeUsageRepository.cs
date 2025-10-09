namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IDataTypeUsageRepository
{
    Task<bool> HasSavedValuesAsync(Guid dataTypeKey);
}
