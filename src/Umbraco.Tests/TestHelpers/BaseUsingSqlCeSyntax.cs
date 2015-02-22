using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.LightInject;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;

namespace Umbraco.Tests.TestHelpers
{
    [TestFixture]
    public abstract class BaseUsingSqlCeSyntax
    {
        private SqlCeSyntaxProvider _sqlSyntax;
        protected SqlCeSyntaxProvider SqlSyntax
        {
            get { return _sqlSyntax ?? (_sqlSyntax = new SqlCeSyntaxProvider()); }
        }

        [SetUp]
        public virtual void Initialize()
        {
            var container = new ServiceContainer();
            container.Register<ISqlSyntaxProvider>(factory => SqlSyntax, new PerContainerLifetime());
            container.Register<ILogger>(factory => Mock.Of<ILogger>(), new PerContainerLifetime());
            container.Register<IProfiler>(factory => Mock.Of<IProfiler>(), new PerContainerLifetime());

            var logger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());
            
            PluginManager.Current = new PluginManager(new ActivatorServiceProvider(), new NullCacheProvider(), 
                logger,
                false);
            MappingResolver.Current = new MappingResolver(
                container, logger.Logger,
                () => PluginManager.Current.ResolveAssignedMapperTypes());

            Resolution.Freeze();
            SetUp();
        }

        public virtual void SetUp()
        {}

        [TearDown]
        public virtual void TearDown()
        {
            MappingResolver.Reset();
            PluginManager.Current = null;
        }
    }
}