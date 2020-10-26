using System;
using System.Collections.Concurrent;
using Moq;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Infrastructure.Persistence.Mappers;
using Umbraco.Persistance.SqlCe;

namespace Umbraco.Tests.Persistence.Mappers
{
    public class MapperTestBase
    {
        protected Lazy<ISqlContext> MockSqlContext()
        {
            var sqlContext = Mock.Of<ISqlContext>();
            var syntax = new SqlCeSyntaxProvider();
            Mock.Get(sqlContext).Setup(x => x.SqlSyntax).Returns(syntax);
            return new Lazy<ISqlContext>(() => sqlContext);
        }

        protected MapperConfigurationStore CreateMaps()
            => new MapperConfigurationStore();
    }
}
