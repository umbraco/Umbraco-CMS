using System;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Options;
using Moq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Persistence.SqlServer.Services;

namespace Umbraco.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class ModelToSqlExpressionHelperBenchmarks
    {
        protected Lazy<ISqlContext> MockSqlContext()
        {
            var sqlContext = Mock.Of<ISqlContext>();
            var syntax = new SqlServerSyntaxProvider(Options.Create(new GlobalSettings()));
            Mock.Get(sqlContext).Setup(x => x.SqlSyntax).Returns(syntax);
            return new Lazy<ISqlContext>(() => sqlContext);
        }

        protected MapperConfigurationStore CreateMaps()
            => new MapperConfigurationStore();

        public ModelToSqlExpressionHelperBenchmarks()
        {
            var contentMapper = new ContentMapper(MockSqlContext(), CreateMaps());
            _cachedExpression = new CachedExpression();
            var mapperCollection = new Mock<IMapperCollection>();
            mapperCollection.Setup(x => x[It.IsAny<Type>()]).Returns(contentMapper);
            _mapperCollection = mapperCollection.Object;
        }

        private readonly ISqlSyntaxProvider _syntaxProvider = new SqlServerSyntaxProvider(Options.Create(new GlobalSettings()));
        private readonly CachedExpression _cachedExpression;
        private readonly IMapperCollection _mapperCollection;

        [Benchmark(Baseline = true)]
        public void WithNonCached()
        {
            for (int i = 0; i < 100; i++)
            {
                var a = i;
                var b = i * 10;
                Expression<Func<IContent, bool>> predicate = content =>
                content.Path.StartsWith("-1") && content.Published && (content.ContentTypeId == a || content.ContentTypeId == b);

                var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IContent>(_syntaxProvider, _mapperCollection);
                var result = modelToSqlExpressionHelper.Visit(predicate);
            }
        }

        [Benchmark]
        public void WithCachedExpression()
        {
            for (int i = 0; i < 100; i++)
            {
                var a = i;
                var b = i * 10;
                Expression<Func<IContent, bool>> predicate = content =>
                content.Path.StartsWith("-1") && content.Published && (content.ContentTypeId == a || content.ContentTypeId == b);

                var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IContent>(_syntaxProvider, _mapperCollection);

                //wrap it!
                _cachedExpression.Wrap(predicate);

                var result = modelToSqlExpressionHelper.Visit(_cachedExpression);
            }
        }
    }
}
