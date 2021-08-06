using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.SqlCe
{
    public class SqlCeSpecificMapperFactory : IProviderSpecificMapperFactory
    {
        public string ProviderName => Constants.DatabaseProviders.SqlCe;
        public NPocoMapperCollection Mappers => new NPocoMapperCollection(() => new[] {new SqlCeImageMapper()});
    }
}
