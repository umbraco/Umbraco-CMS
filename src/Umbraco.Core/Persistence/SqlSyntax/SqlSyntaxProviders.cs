using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    // fixme - this exists ONLY for unit tests at the moment!
    public sealed class SqlSyntaxProviders 
    {
        public static IEnumerable<ISqlSyntaxProvider> GetDefaultProviders(ILogger logger)
        {
            return new ISqlSyntaxProvider[]
            {
                new MySqlSyntaxProvider(logger),
                new SqlCeSyntaxProvider(),
                new SqlServerSyntaxProvider(new Lazy<IDatabaseFactory>(() => null))
            };
        }
    }
}