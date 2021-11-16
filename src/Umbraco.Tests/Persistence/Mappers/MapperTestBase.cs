using System;
using System.Collections.Concurrent;
using Moq;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

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

        protected ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> CreateMaps()
            => new ConcurrentDictionary<Type, ConcurrentDictionary<string, string>>();
    }
}
