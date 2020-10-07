using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Composing
{
    [TestFixture]
    public class CompositionTests
    {
        [Test]
        public void FactoryIsResolvable()
        {
            Func<IFactory, IFactory> factoryFactory = null;

            var mockedRegister = Mock.Of<IRegister>();
            var mockedFactory = Mock.Of<IFactory>();

            // the mocked register creates the mocked factory
            Mock.Get(mockedRegister)
                .Setup(x => x.CreateFactory())
                .Returns(mockedFactory);

            // the mocked register can register a factory factory
            Mock.Get(mockedRegister)
                .Setup(x => x.Register(It.IsAny<Func<IFactory, IFactory>>(), Lifetime.Singleton))
                .Callback<Func<IFactory, IFactory>, Lifetime>((ff, lt) => factoryFactory = ff);

            // the mocked factory can invoke the factory factory
            Mock.Get(mockedFactory)
                .Setup(x => x.GetInstance(typeof(IFactory)))
                .Returns(() => factoryFactory?.Invoke(mockedFactory));

            var logger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());
            var typeFinder = TestHelper.GetTypeFinder();
            var ioHelper = TestHelper.IOHelper;
            var typeLoader = new TypeLoader(typeFinder, Mock.Of<IAppPolicyCache>(), new DirectoryInfo(ioHelper.MapPath("~/App_Data/TEMP")), Mock.Of<ILogger<TypeLoader>>(), logger);
            var composition = new Composition(mockedRegister, typeLoader, logger, Mock.Of<IRuntimeState>(), TestHelper.IOHelper, AppCaches.NoCache);

            // create the factory, ensure it is the mocked factory
            var factory = composition.CreateFactory();
            Assert.AreSame(mockedFactory, factory);

            // ensure we can get an IFactory instance,
            // meaning that it has been properly registered

            var resolved = factory.GetInstance<IFactory>();
            Assert.IsNotNull(resolved);
            Assert.AreSame(factory, resolved);
        }
    }
}
