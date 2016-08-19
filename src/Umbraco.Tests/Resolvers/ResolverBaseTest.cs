using System.Collections.Generic;
using System.Reflection;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Plugins;
using Umbraco.Core.Profiling;
using Umbraco.Core._Legacy.PackageActions;

namespace Umbraco.Tests.Resolvers
{
    public abstract class ResolverBaseTest
    {
        protected PluginManager PluginManager { get; private set; }
        protected ProfilingLogger ProfilingLogger { get; private set; }

        [SetUp]
        public void Initialize()
        {
            ProfilingLogger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());

            PluginManager = new PluginManager(new NullCacheProvider(),
                ProfilingLogger,
                false)
            {
                AssembliesToScan = AssembliesToScan
            };
        }

        [TearDown]
        public void TearDown()
        {
            Resolution.Reset();
            Current.Reset();
        }

        protected virtual IEnumerable<Assembly> AssembliesToScan
        {
            get
            {
                return new[]
                {
                    this.GetType().Assembly // this assembly only
                };
            }
        }
    }
}