using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Composing
{
    public abstract class ComposingTestBase
    {
        protected TypeLoader TypeLoader { get; private set; }

        protected IProfilingLogger ProfilingLogger { get; private set; }

        [SetUp]
        public void Initialize()
        {
            ProfilingLogger = new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>());

            var typeFinder = TestHelper.GetTypeFinder();
            var ioHelper = TestHelper.IOHelper;
            TypeLoader = new TypeLoader(typeFinder, NoAppCache.Instance, new DirectoryInfo(ioHelper.MapPath("~/App_Data/TEMP")), Mock.Of<ILogger<TypeLoader>>(), ProfilingLogger, false, AssembliesToScan);
        }

        protected virtual IEnumerable<Assembly> AssembliesToScan
            => new[]
            {
                GetType().Assembly // this assembly only
            };
    }
}
