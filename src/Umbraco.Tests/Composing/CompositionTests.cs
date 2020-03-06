using System;
using System.Reflection;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

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
            var typeLoader = new TypeLoader(Mock.Of<IAppPolicyCache>(), IOHelper.MapPath("~/App_Data/TEMP"), logger);
            var composition = new Composition(mockedRegister, typeLoader, logger, Mock.Of<IRuntimeState>());

            // create the factory, ensure it is the mocked factory
            var factory = composition.CreateFactory();
            Assert.AreSame(mockedFactory, factory);

            // ensure we can get an IFactory instance,
            // meaning that it has been properly registered

            var resolved = factory.GetInstance<IFactory>();
            Assert.IsNotNull(resolved);
            Assert.AreSame(factory, resolved);
        }

        [Test]
        public void Users_Can_Resolve_Instances_Registered_By_Convention()
        {
            var mockedProfilingLogger = Mock.Of<IProfilingLogger>();
            var mockedRuntimeState = Mock.Of<IRuntimeState>();

            Mock.Get(mockedRuntimeState).Setup(x => x.Level)
                .Returns(RuntimeLevel.Run);

            var composition = new Composition(
                LightInjectContainer.Create(),
                null, // This is not used.
                mockedProfilingLogger,
                mockedRuntimeState);

            var composers = new Composers(
                composition,
                new [] {typeof(StubUserComposer)},
                new Attribute[0],
                mockedProfilingLogger);

            composers.Compose();

            var factory = composition.CreateFactory();
            var instance = factory.GetInstance<IConventionRegisteredAndResolvable>();

            Assert.NotNull(instance);
            Assert.IsInstanceOf<ConventionRegisteredAndResolvable>(instance);
        }

        [Test]
        public void Container_Will_Scan_When_Explicitly_Told_To_Do_So()
        {
            var mockedProfilingLogger = Mock.Of<IProfilingLogger>();
            var mockedRuntimeState = Mock.Of<IRuntimeState>();
            var mockedAssemblyScanner = Mock.Of<IAssemblyScanner>();

            Mock.Get(mockedRuntimeState).Setup(x => x.Level)
                .Returns(RuntimeLevel.Run);

            var outer = LightInjectContainer.Create();
            var container = (ServiceContainer)outer.Concrete;

            container.AssemblyScanner = mockedAssemblyScanner;

            var composition = new Composition(
                outer,
                null, // This is not used.
                mockedProfilingLogger,
                mockedRuntimeState);

            var composers = new Composers(
                composition,
                new[] { typeof(StubUserComposer) },
                new Attribute[0],
                mockedProfilingLogger);

            composers.Compose();

            Mock.Get(mockedAssemblyScanner)
                .Verify(scanner => scanner.Scan(
                    It.IsAny<Assembly>(),
                    It.IsAny<IServiceRegistry>(),
                    It.IsAny<Func<ILifetime>>(),
                    It.IsAny<Func<Type, Type, bool>>(),
                    It.IsAny<Func<Type, Type, string>>()
                ), Times.Once);
        }

        [Test]
        public void Container_Does_Not_Scan_For_Composition_Roots_When_Resolving_Unregistered_Types()
        {
            var mockedProfilingLogger = Mock.Of<IProfilingLogger>();
            var mockedRuntimeState = Mock.Of<IRuntimeState>();
            var mockedAssemblyScanner = Mock.Of<IAssemblyScanner>();

            var outer = LightInjectContainer.Create();
            var container = (ServiceContainer) outer.Concrete;

            container.AssemblyScanner = mockedAssemblyScanner;

            var composition = new Composition(
                outer,
                null, // This is not used.
                mockedProfilingLogger,
                mockedRuntimeState);

            var composers = new Composers(
                composition,
                new Type[0],
                new Attribute[0],
                mockedProfilingLogger);

            composers.Compose();

            var factory = composition.CreateFactory();
            Assert.IsNull(factory.GetInstance<INeverGotRegistered>());

            Mock.Get(mockedAssemblyScanner)
                .Verify(scanner => scanner.Scan(
                    It.IsAny<Assembly>(),
                    It.IsAny<IServiceRegistry>()
                ), Times.Never);

            Mock.Get(mockedAssemblyScanner)
                .Verify(scanner => scanner.Scan(
                    It.IsAny<Assembly>(),
                    It.IsAny<IServiceRegistry>(),
                    It.IsAny<Func<ILifetime>>(),
                    It.IsAny<Func<Type, Type, bool>>(),
                    It.IsAny<Func<Type, Type, string>>()
                ), Times.Never);
        }

        public class StubUserComposer : IUserComposer
        {
            public void Compose(Composition composition)
            {
                var container = (ServiceContainer)composition.Concrete;

                container.RegisterAssembly(typeof(IConventionRegisteredAndResolvable).Assembly);
            }
        }

        public interface IConventionRegisteredAndResolvable { }
        public class ConventionRegisteredAndResolvable : IConventionRegisteredAndResolvable { }

        public interface INeverGotRegistered { }
        public class NeverGotRegistered : INeverGotRegistered { }
    }
}
