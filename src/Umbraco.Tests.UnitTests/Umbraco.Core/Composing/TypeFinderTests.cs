using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web.BackOffice.Trees;

namespace Umbraco.Tests.Composing
{

    /// <summary>
    /// Tests for typefinder
    /// </summary>
    [TestFixture]
    public class TypeFinderTests
    {
        /// <summary>
        /// List of assemblies to scan
        /// </summary>
        private Assembly[] _assemblies;

        [SetUp]
        public void Initialize()
        {
            _assemblies = new[]
                {
                    this.GetType().Assembly,
                    typeof(System.Guid).Assembly,
                    typeof(NUnit.Framework.Assert).Assembly,
                    typeof(System.Xml.NameTable).Assembly,
                    typeof(System.Configuration.GenericEnumConverter).Assembly,
                    typeof(TypeFinder).Assembly,
                };

        }

        [Test]
        public void Find_Class_Of_Type_With_Attribute()
        {
            var typeFinder = new TypeFinder(Mock.Of<ILogger<TypeFinder>>(), new DefaultUmbracoAssemblyProvider(GetType().Assembly), new VaryingRuntimeHash());
            var typesFound = typeFinder.FindClassesOfTypeWithAttribute<TestEditor, MyTestAttribute>(_assemblies);
            Assert.AreEqual(2, typesFound.Count());
        }

        [Test]
        public void Find_Classes_With_Attribute()
        {
            var typeFinder = new TypeFinder(Mock.Of<ILogger<TypeFinder>>(), new DefaultUmbracoAssemblyProvider(GetType().Assembly), new VaryingRuntimeHash());
            var typesFound = typeFinder.FindClassesWithAttribute<TreeAttribute>(_assemblies);
            Assert.AreEqual(0, typesFound.Count()); // 0 classes in _assemblies are marked with [Tree]

            typesFound = typeFinder.FindClassesWithAttribute<TreeAttribute>(new[] { typeof (TreeAttribute).Assembly });
            Assert.AreEqual(22, typesFound.Count()); // + classes in Umbraco.Web are marked with [Tree]

            typesFound = typeFinder.FindClassesWithAttribute<TreeAttribute>();
            Assert.AreEqual(22, typesFound.Count()); // + classes in Umbraco.Web are marked with [Tree]
        }

        private static IProfilingLogger GetTestProfilingLogger()
        {
            var logger = LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger("ProfilingLogger");
            var profiler = new TestProfiler();
            return new ProfilingLogger(logger, profiler);
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        public class MyTestAttribute : Attribute
        {

        }

        public abstract class TestEditor
        {

        }

        [MyTest]
        public class BenchmarkTestEditor : TestEditor
        {

        }

        [MyTest]
        public class MyOtherTestEditor : TestEditor
        {

        }

    }


}
