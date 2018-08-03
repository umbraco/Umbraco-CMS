using System;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
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
            var contentMapper = new ContentMapper(_syntaxProvider);
            contentMapper.BuildMap();
            _cachedExpression = new CachedExpression();
            var mappingResolver = new Mock<MappingResolver>();
            mappingResolver.Setup(resolver => resolver.ResolveMapperByType(It.IsAny<Type>())).Returns(contentMapper);
            _mappingResolver = mappingResolver.Object;
        }

        private readonly ISqlSyntaxProvider _syntaxProvider = new SqlCeSyntaxProvider();
        private readonly CachedExpression _cachedExpression;
        private readonly MappingResolver _mappingResolver;

        [Benchmark(Baseline = true)]
        public void WithNonCached()
        {
            for (int i = 0; i < 100; i++)
            {
                var a = i;
                var b = i * 10;
                Expression<Func<IContent, bool>> predicate = content =>
                content.Path.StartsWith("-1") && content.Published && (content.ContentTypeId == a || content.ContentTypeId == b);

                var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IContent>(_syntaxProvider, _mappingResolver);
                var result = modelToSqlExpressionHelper.Visit(predicate);
            }
        }

        [Benchmark()]
        public void WithCachedExpression()
        {
            for (int i = 0; i < 100; i++)
            {
                var a = i;
                var b = i * 10;
                Expression<Func<IContent, bool>> predicate = content =>
                content.Path.StartsWith("-1") && content.Published && (content.ContentTypeId == a || content.ContentTypeId == b);

                var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IContent>(_syntaxProvider, _mappingResolver);

                //wrap it!
                _cachedExpression.Wrap(predicate);

                var result = modelToSqlExpressionHelper.Visit(_cachedExpression);
            }
        }
    }
}