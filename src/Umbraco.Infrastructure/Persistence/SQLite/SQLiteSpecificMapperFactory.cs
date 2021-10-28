using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.SQLite
{
    public class SQLiteSpecificMapperFactory : IProviderSpecificMapperFactory
    {
        public string ProviderName => Constants.DatabaseProviders.SQLite;
        public NPocoMapperCollection Mappers => new NPocoMapperCollection(() => new[] { new SQLiteGuidMapper() });
    }
}
