namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IPropertyTypeUsageRepository
{
    bool HasValues(string propertyTypeAlias);
}
