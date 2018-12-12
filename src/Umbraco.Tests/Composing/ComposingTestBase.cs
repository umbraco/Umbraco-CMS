using System.Collections.Generic;
using System.Reflection;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Composing
{
    public abstract class ComposingTestBase
    {
        protected TypeLoader TypeLoader { get; private set; }

        protected IProfilingLogger ProfilingLogger { get; private set; }

        [SetUp]
        public void Initialize()
        {
            ProfilingLogger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());

            TypeLoader = new TypeLoader(NullCacheProvider.Instance, LocalTempStorage.Default, ProfilingLogger, detectChanges: false)
            {
                AssembliesToScan = AssembliesToScan
            };
        }

        [TearDown]
        public void TearDown()
        {
            Current.Reset();
        }

        protected virtual IEnumerable<Assembly> AssembliesToScan
            => new[]
            {
                GetType().Assembly // this assembly only
            };
    }
}
