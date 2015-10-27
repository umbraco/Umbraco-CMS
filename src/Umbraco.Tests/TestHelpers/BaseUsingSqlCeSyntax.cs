using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
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
        [SetUp]
        public virtual void Initialize()
        {
            var logger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();
            PluginManager.Current = new PluginManager(new ActivatorServiceProvider(), new NullCacheProvider(), 
                logger,
                false);
            MappingResolver.Current = new MappingResolver(
                new ActivatorServiceProvider(), logger.Logger,
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
            SqlSyntaxContext.SqlSyntaxProvider = null;
            PluginManager.Current = null;
        }
    }
}