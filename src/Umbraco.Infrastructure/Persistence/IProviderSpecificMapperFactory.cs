namespace Umbraco.Cms.Infrastructure.Persistence;

public interface IProviderSpecificMapperFactory
{
    string ProviderName { get; }

    NPocoMapperCollection Mappers { get; }
}
