using System.Collections.Generic;
using System.Reflection;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Profiling;

namespace Umbraco.Tests.Resolvers
{
    public abstract class ResolverBaseTest
    {
        protected PluginManager PluginManager { get; private set; }
        protected ProfilingLogger ProfilingLogger { get; private set; }

        [SetUp]
        public void Initialize()
        {

            PackageActionsResolver.Reset();

            ProfilingLogger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());

            PluginManager = new PluginManager(new ActivatorServiceProvider(), new NullCacheProvider(),
                ProfilingLogger,
                false)
            {
                AssembliesToScan = AssembliesToScan
            };
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