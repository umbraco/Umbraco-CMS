﻿using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.DependencyInjection;

namespace Umbraco.Tests.TestHelpers
{
    [TestFixture]
    public abstract class BaseUsingSqlCeSyntax
    {
        protected virtual SqlCeSyntaxProvider SqlSyntax
        {
            get { return new SqlCeSyntaxProvider(); }
        }

        private MappingResolver _mappingResolver;
        protected IMappingResolver MappingResolver
        {
            get { return _mappingResolver; }
        }

        [SetUp]
        public virtual void Initialize()
        {
            var container = new ServiceContainer();
            container.RegisterSingleton<ISqlSyntaxProvider>(factory => SqlSyntax);
            container.RegisterSingleton<ILogger>(factory => Mock.Of<ILogger>());
            container.RegisterSingleton<IProfiler>(factory => Mock.Of<IProfiler>());

            _mappingResolver = new MappingResolver(container, Mock.Of<ILogger>(),
                () => PluginManager.Current.ResolveAssignedMapperTypes());

            var logger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());
            
            PluginManager.Current = new PluginManager(new ActivatorServiceProvider(), new NullCacheProvider(), 
                logger,
                false);

            Resolution.Freeze();
            SetUp();
        }

        public virtual void SetUp()
        {}

        [TearDown]
        public virtual void TearDown()
        {
            Resolution.Reset();
            //MappingResolver.Reset();
            PluginManager.Current = null;
        }
    }
}