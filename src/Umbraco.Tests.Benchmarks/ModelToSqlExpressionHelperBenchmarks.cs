using System;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Moq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class ModelToSqlExpressionHelperBenchmarks
    {
        public ModelToSqlExpressionHelperBenchmarks()
        {
            var contentMapper = new ContentMapper();
            _cachedExpression = new CachedExpression();
            var mapperCollection = new Mock<IMapperCollection>();
            mapperCollection.Setup(x => x[It.IsAny<Type>()]).Returns(contentMapper);
            _mapperCollection = mapperCollection.Object;
        }

        private readonly ISqlSyntaxProvider _syntaxProvider = new SqlCeSyntaxProvider();
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
