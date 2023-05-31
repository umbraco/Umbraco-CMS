using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.SqlServer.Services;

public class SqlServerSpecificMapperFactory : IProviderSpecificMapperFactory
{
    public string ProviderName => Constants.ProviderName;

    public NPocoMapperCollection Mappers => new(() => new[] { new UmbracoDefaultMapper() });
}
