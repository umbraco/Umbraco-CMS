using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.Trees;

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
                    typeof(Microsoft.CSharp.CSharpCodeProvider).Assembly,
                    typeof(System.Xml.NameTable).Assembly,
                    typeof(System.Configuration.GenericEnumConverter).Assembly,
                    typeof(System.Web.SiteMap).Assembly,
                    //typeof(TabPage).Assembly,
                    typeof(System.Web.Mvc.ActionResult).Assembly,
                    typeof(TypeFinder).Assembly,
                    typeof(global::Umbraco.Examine.UmbracoExamineIndex).Assembly
                };

        }

        [Test]
        public void Find_Class_Of_Type_With_Attribute()
        {
            var typeFinder = new TypeFinder(GetTestProfilingLogger());
            var typesFound = typeFinder.FindClassesOfTypeWithAttribute<TestEditor, MyTestAttribute>(_assemblies);
            Assert.AreEqual(2, typesFound.Count());
        }

        [Test]
        public void Find_Classes_With_Attribute()
        {
            var typeFinder = new TypeFinder(GetTestProfilingLogger());
            var typesFound = typeFinder.FindClassesWithAttribute<TreeAttribute>(_assemblies);
            Assert.AreEqual(0, typesFound.Count()); // 0 classes in _assemblies are marked with [Tree]

            typesFound = typeFinder.FindClassesWithAttribute<TreeAttribute>(new[] { typeof (UmbracoContext).Assembly });
            Assert.AreEqual(22, typesFound.Count()); // + classes in Umbraco.Web are marked with [Tree]
        }

        private static IProfilingLogger GetTestProfilingLogger()
        {
            var logger = new DebugDiagnosticsLogger(new MessageTemplates());
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
