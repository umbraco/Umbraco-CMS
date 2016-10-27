using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Xml;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;
using ILogger = Umbraco.Core.Logging.ILogger;

namespace Umbraco.Tests.Benchmarks
{
    [Config(typeof(Config))]
    public class ModelToSqlExpressionHelperBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                Add(new MemoryDiagnoser());                
            }
        }

        public ModelToSqlExpressionHelperBenchmarks()
        {
            _contentMapper = new ContentMapper(_syntaxProvider);
            _contentMapper.BuildMap();
            _cachedExpression = new CachedExpression();
        }
        
        private readonly ISqlSyntaxProvider _syntaxProvider = new SqlCeSyntaxProvider();
        private readonly BaseMapper _contentMapper;
        private readonly CachedExpression _cachedExpression;
        //private static readonly Expression<Func<IContent, bool>> TemplatePredicate = content =>
        //        content.Path.StartsWith(string.Empty) && content.Published && (content.ContentTypeId == 0 || content.ContentTypeId == 0);

        [Benchmark(Baseline = true)]
        public void WithNonCached()
        {
            for (int i = 0; i < 100; i++)
            {
                var a = i;
                var b = i*10;
                Expression<Func<IContent, bool>> predicate = content =>
                content.Path.StartsWith("-1") && content.Published && (content.ContentTypeId == a || content.ContentTypeId == b);

                var modelToSqlExpressionHelper = new ModelToSqlExpressionHelper<IContent>(_syntaxProvider, _contentMapper);
                var result = modelToSqlExpressionHelper.Visit(predicate);
            }
            
        }

        [Benchmark]
        public void WithSQL()
        {
            var subQuery = new Sql()
                        //.Select("umbracoNode.id as nodeId")
                        .From<ContentDto>(_syntaxProvider)
                        .InnerJoin<NodeDto>(_syntaxProvider)
                        .On<ContentDto, NodeDto>(_syntaxProvider, left => left.NodeId, right => right.NodeId)
                        .WhereIn<ContentDto>(dto => dto.ContentTypeId, contentTypeIds, SqlSyntax)
                        .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
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
                
                var modelToSqlExpressionHelper = new ModelToSqlExpressionHelper<IContent>(_syntaxProvider, _contentMapper);

                //wrap it!
                _cachedExpression.Wrap(predicate);

                var result = modelToSqlExpressionHelper.Visit(_cachedExpression);
            }
        }

    }
}