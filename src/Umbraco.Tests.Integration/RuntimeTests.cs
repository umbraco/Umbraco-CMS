using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Runtime;
using Umbraco.Tests.Integration.Implementations;

namespace Umbraco.Tests.Integration
{
    [TestFixture]
    public class RuntimeTests
    {
        [Test]
        public void BootCoreRuntime()
        {
            // MSDI
            var services = new ServiceCollection();

            // LightInject / Umbraco
            var umbracoContainer = RegisterFactory.CreateFrom(services, out var lightInjectServiceProvider);

            // Dependencies needed for Core Runtime
            var testHelper = new TestHelper();

            var coreRuntime = new CoreRuntime(testHelper.GetConfigs(), testHelper.GetUmbracoVersion(),
                testHelper.IOHelper, testHelper.Logger, testHelper.Profiler, testHelper.UmbracoBootPermissionChecker,
                testHelper.GetHostingEnvironment(), testHelper.GetBackOfficeInfo(), testHelper.DbProviderFactoryCreator,
                testHelper.MainDom, testHelper.GetTypeFinder());

            var factory = coreRuntime.Boot(umbracoContainer);

            Assert.IsTrue(coreRuntime.MainDom.IsMainDom);
            Assert.IsNull(coreRuntime.State.BootFailedException);
            Assert.AreEqual(RuntimeLevel.Install, coreRuntime.State.Level);            
            Assert.IsTrue(MyComposer.IsComposed);
        }        
    }

    [RuntimeLevel(MinLevel = RuntimeLevel.Install)]
    public class MyComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            IsComposed = true;
        }

        public static bool IsComposed { get; private set; }
    }
}
