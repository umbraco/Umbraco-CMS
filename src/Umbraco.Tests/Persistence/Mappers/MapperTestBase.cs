using System;
using Moq;
using Umbraco.Core.Persistence;
using Umbraco.Infrastructure.Persistence.Mappers;
using Umbraco.Persistence.SqlCe;

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
