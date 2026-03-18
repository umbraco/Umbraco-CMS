// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Components;

/// <summary>
/// Contains unit tests for the core components of Umbraco.
/// </summary>
[TestFixture]
public class ComponentTests
{
    private static readonly List<Type> Composed = new();
    private static readonly IIOHelper IOHelper = TestHelper.IOHelper;
    private static readonly List<Type> Initialized = new();
    private static readonly List<Type> Terminated = new();

    private static IServiceProvider MockFactory(Action<Mock<IServiceProvider>> setup = null)
    {
        // TODO: use IUmbracoDatabaseFactory vs UmbracoDatabaseFactory, clean it all up!
        var mock = new Mock<IServiceProvider>();
        ILoggerFactory loggerFactory = NullLoggerFactory.Instance;
        var logger = loggerFactory.CreateLogger("GenericLogger");
        var globalSettings = new GlobalSettings();
        var connectionStrings = new ConnectionStrings();
        var mapperCollection = new NPocoMapperCollection(() => new[] { new NullableDateMapper() });
        var f = new UmbracoDatabaseFactory(
            loggerFactory.CreateLogger<UmbracoDatabaseFactory>(),
            loggerFactory,
            Options.Create(globalSettings),
            Mock.Of<IOptionsMonitor<ConnectionStrings>>(x => x.Get(It.IsAny<string>()) == connectionStrings),
            new MapperCollection(() => Enumerable.Empty<BaseMapper>()),
            Mock.Of<IDbProviderFactoryCreator>(),
            new DatabaseSchemaCreatorFactory(
                loggerFactory.CreateLogger<DatabaseSchemaCreator>(),
                loggerFactory,
                new UmbracoVersion(),
                Mock.Of<IEventAggregator>(),
                Mock.Of<IOptionsMonitor<InstallDefaultDataSettings>>()),
            mapperCollection);

        var fs = new FileSystems(
            loggerFactory,
            IOHelper,
            Options.Create(globalSettings),
            Mock.Of<IHostingEnvironment>());
        var coreDebug = new CoreDebugSettings();
        var mediaFileManager = new MediaFileManager(
            Mock.Of<IFileSystem>(),
            Mock.Of<IMediaPathScheme>(),
            Mock.Of<ILogger<MediaFileManager>>(),
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<Lazy<ICoreScopeProvider>>());
        var eventAggregator = Mock.Of<IEventAggregator>();
        var scopeProvider = new ScopeProvider(
            new AmbientScopeStack(),
            new AmbientScopeContextStack(),
            Mock.Of<IDistributedLockingMechanismFactory>(),
            f,
            fs,
            new TestOptionsMonitor<CoreDebugSettings>(coreDebug),
            mediaFileManager,
            loggerFactory,

            eventAggregator);

        mock.Setup(x => x.GetService(typeof(ILogger))).Returns(logger);
        mock.Setup(x => x.GetService(typeof(ILogger<ComponentCollection>)))
            .Returns(loggerFactory.CreateLogger<ComponentCollection>);
        mock.Setup(x => x.GetService(typeof(ILoggerFactory))).Returns(loggerFactory);
        mock.Setup(x => x.GetService(typeof(IProfilingLogger)))
            .Returns(new ProfilingLogger(loggerFactory.CreateLogger<ProfilingLogger>(), Mock.Of<IProfiler>()));
        mock.Setup(x => x.GetService(typeof(IUmbracoDatabaseFactory))).Returns(f);
        mock.Setup(x => x.GetService(typeof(ICoreScopeProvider))).Returns(scopeProvider);

        setup?.Invoke(mock);
        return mock.Object;
    }

    private static IServiceCollection MockRegister() => new ServiceCollection();

    private static TypeLoader MockTypeLoader() => new(
        Mock.Of<ITypeFinder>(),
        Mock.Of<ILogger<TypeLoader>>());

    /// <summary>
    /// Tests the bootstrapping and composition process of components using multiple composers.
    /// Verifies that components are composed in the correct order based on dependencies, and ensures that
    /// the initialization and termination methods are called as expected.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Boot1A()
    {
        var register = MockRegister();
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        var types = TypeArray<Composer1, Composer2, Composer4>();
        var composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();

        // 2 is Core and requires 4
        // 3 is User
        // => reorder components accordingly
        composers.Compose();
        AssertTypeArray(TypeArray<Composer1, Composer4, Composer2>(), Composed);

        var factory = MockFactory(m =>
        {
            m.Setup(x => x.GetService(It.Is<Type>(t => t == typeof(ISomeResource)))).Returns(() => new SomeResource());
            m.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(type =>
            {
                if (type == typeof(Composer1))
                {
                    return new Composer1();
                }

                if (type == typeof(IProfilingLogger))
                {
                    return new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>());
                }

                if (type == typeof(ILogger<ComponentCollection>))
                {
                    return Mock.Of<ILogger<ComponentCollection>>();
                }

                if (type == typeof(ILogger<ComponentCollection>))
                {
                    return Mock.Of<ILogger<ComponentCollection>>();
                }

                if (type == typeof(IServiceProviderIsService))
                {
                    return Mock.Of<IServiceProviderIsService>();
                }
                throw new NotSupportedException(type.FullName);
            });
        });

        var builder = composition.WithCollectionBuilder<ComponentCollectionBuilder>();
        builder.RegisterWith(register);
        var components = builder.CreateCollection(factory);

        Assert.IsEmpty(components);
        await components.InitializeAsync(false, default);
        Assert.IsEmpty(Initialized);
        await components.TerminateAsync(false, default);
        Assert.IsEmpty(Terminated);
    }

    /// <summary>
    /// Tests the composition and ordering of composers in the UmbracoBuilder.
    /// Ensures that composers are reordered correctly based on dependencies.
    /// </summary>
    [Test]
    public void Boot1B()
    {
        var register = MockRegister();
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        var types = TypeArray<Composer1, Composer2, Composer3, Composer4>();
        var composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();

        // 2 is Core and requires 4
        // 3 is User - stays with RuntimeLevel.Run
        // => reorder components accordingly
        composers.Compose();
        AssertTypeArray(TypeArray<Composer1, Composer4, Composer2, Composer3>(), Composed);
    }

    /// <summary>
    /// Verifies that the bootstrapping process respects composer dependencies by ensuring components are composed in the correct order.
    /// Specifically, tests that a component required by another is composed first.
    /// </summary>
    [Test]
    public void Boot2()
    {
        var register = MockRegister();
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        var types = TypeArray<Composer20, Composer21>();
        var composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();

        // 21 is required by 20
        // => reorder components accordingly
        composers.Compose();
        AssertTypeArray(TypeArray<Composer21, Composer20>(), Composed);
    }

    /// <summary>
    /// Tests the composition and ordering of composers with dependencies.
    /// </summary>
    [Test]
    public void Boot3()
    {
        var register = MockRegister();
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        var types = TypeArray<Composer22, Composer24, Composer25>();
        var composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();

        // i23 requires 22
        // 24, 25 implement i23
        // 25 required by i23
        // => reorder components accordingly
        composers.Compose();
        AssertTypeArray(TypeArray<Composer22, Composer25, Composer24>(), Composed);
    }

    /// <summary>
    /// Tests that a broken composer dependency (missing required composer) throws an exception.
    /// </summary>
    [Test]
    public void BrokenRequire()
    {
        var register = MockRegister();
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        var types = TypeArray<Composer1, Composer2, Composer3>();
        var composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();
        try
        {
            // 2 is Core and requires 4
            // 4 is missing
            // => throw
            composers.Compose();
            Assert.Fail("Expected exception.");
        }
        catch (Exception e)
        {
            Assert.AreEqual(
                "Broken composer dependency: Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Components.ComponentTests+Composer2 -> Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Components.ComponentTests+Composer4.",
                e.Message);
        }
    }

    /// <summary>
    /// Verifies the component composition logic when required components are missing or broken.
    /// Specifically, tests that composers are reordered according to their dependencies, and that the absence
    /// of a required component (e.g., Composer1) is handled gracefully, ensuring the remaining composers are composed in the correct order.
    /// </summary>
    [Test]
    public void BrokenRequired()
    {
        var register = MockRegister();
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        var types = TypeArray<Composer2, Composer4, Composer13>();
        var composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();

        // 2 is Core and requires 4
        // 13 is required by 1
        // 1 is missing
        // => reorder components accordingly
        composers.Compose();
        AssertTypeArray(TypeArray<Composer4, Composer2, Composer13>(), Composed);
    }

    /// <summary>
    /// Initializes the test environment for component tests by clearing state, setting up mocks, and composing test components.
    /// This method prepares the necessary dependencies and verifies the composition process for components in the test context.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    [Test]
    public async Task Initialize()
    {
        Composed.Clear();
        Initialized.Clear();
        Terminated.Clear();

        var register = MockRegister();
        var typeLoader = MockTypeLoader();
        var factory = MockFactory(m =>
        {
            m.Setup(x => x.GetService(It.Is<Type>(t => t == typeof(ISomeResource)))).Returns(() => new SomeResource());
            m.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(type =>
            {
                if (type == typeof(Composer1))
                {
                    return new Composer1();
                }

                if (type == typeof(IProfilingLogger))
                {
                    return new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>());
                }

                if (type == typeof(ILogger<ComponentCollection>))
                {
                    return Mock.Of<ILogger<ComponentCollection>>();
                }

                if (type == typeof(IServiceProviderIsService))
                {
                    return Mock.Of<IServiceProviderIsService>();
                }

                throw new NotSupportedException(type.FullName);
            });
        });
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        Type[] types = { typeof(Composer1) };
        var composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());

        Assert.IsEmpty(Composed);
        composers.Compose();

        var builder = composition.WithCollectionBuilder<ComponentCollectionBuilder>();
        builder.RegisterWith(register);
        var components = builder.CreateCollection(factory);
    }

    /// <summary>
    /// Tests the composition of multiple composers and verifies the order and count of composed items.
    /// </summary>
    [Test]
    public void Requires1()
    {
        var register = MockRegister();
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        Type[] types = { typeof(Composer6), typeof(Composer7), typeof(Composer8) };
        var composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();
        composers.Compose();
        Assert.AreEqual(2, Composed.Count);
        Assert.AreEqual(typeof(Composer6), Composed[0]);
        Assert.AreEqual(typeof(Composer8), Composed[1]);
    }

    /// <summary>
    /// Tests that composers are composed in the correct order based on their dependencies and requirements.
    /// Ensures that the composition logic respects the required order among Composer9, Composer2, and Composer4.
    /// </summary>
    [Test]
    public void Requires2A()
    {
        var register = MockRegister();
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        Type[] types = { typeof(Composer9), typeof(Composer2), typeof(Composer4) };
        var composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();
        composers.Compose();
        Assert.AreEqual(3, Composed.Count);
        Assert.AreEqual(typeof(Composer4), Composed[0]);
        Assert.AreEqual(typeof(Composer2), Composed[1]);
    }

    /// <summary>
    /// Verifies that components are composed and registered in the correct order when using <see cref="UmbracoBuilder"/> and <see cref="ComposerGraph"/>.
    /// This test specifically checks that the required dependencies between composers (Composer9, Composer2, Composer4) are respected,
    /// and asserts that the resulting composition order matches the expected sequence.
    /// </summary>
    [Test]
    public void Requires2B()
    {
        var register = MockRegister();
        var typeLoader = MockTypeLoader();
        var factory = MockFactory();
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        Type[] types = { typeof(Composer9), typeof(Composer2), typeof(Composer4) };
        var composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();
        composers.Compose();
        var builder = composition.WithCollectionBuilder<ComponentCollectionBuilder>();
        builder.RegisterWith(register);
        var components = builder.CreateCollection(factory);
        Assert.AreEqual(3, Composed.Count);
        Assert.AreEqual(typeof(Composer4), Composed[0]);
        Assert.AreEqual(typeof(Composer2), Composed[1]);
        Assert.AreEqual(typeof(Composer9), Composed[2]);
    }

    /// <summary>
    /// Tests the behavior of weak dependencies in the composition of Umbraco components.
    /// Ensures that composers with weak dependencies compose correctly and that exceptions are thrown when expected.
    /// </summary>
    [Test]
    public void WeakDependencies()
    {
        var register = MockRegister();
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        Type[] types = { typeof(Composer10) };
        var composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();
        composers.Compose();
        Assert.AreEqual(1, Composed.Count);
        Assert.AreEqual(typeof(Composer10), Composed[0]);

        types = new[] { typeof(Composer11) };
        composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();
        Assert.Throws<Exception>(() => composers.Compose());
        Console.WriteLine("throws:");
        composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        var requirements = composers.GetRequirements(false);
        Console.WriteLine(ComposerGraph.GetComposersReport(requirements));

        types = new[] { typeof(Composer2) };
        composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();
        Assert.Throws<Exception>(() => composers.Compose());
        Console.WriteLine("throws:");
        composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        requirements = composers.GetRequirements(false);
        Console.WriteLine(ComposerGraph.GetComposersReport(requirements));

        types = new[] { typeof(Composer12) };
        composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();
        composers.Compose();
        Assert.AreEqual(1, Composed.Count);
        Assert.AreEqual(typeof(Composer12), Composed[0]);
    }

    /// <summary>
    /// Verifies that when a composer disables another composer that is missing from the list,
    /// the composition process completes successfully and only the present composers are composed.
    /// Ensures that missing composers referenced in disable attributes do not cause errors.
    /// </summary>
    [Test]
    public void DisableMissing()
    {
        var register = MockRegister();
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        Type[] types = { typeof(Composer6), typeof(Composer8) }; // 8 disables 7 which is not in the list
        var composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();
        composers.Compose();
        Assert.AreEqual(2, Composed.Count);
        Assert.AreEqual(typeof(Composer6), Composed[0]);
        Assert.AreEqual(typeof(Composer8), Composed[1]);
    }

    /// <summary>
    /// Tests the effect of attribute priorities on component composition.
    /// Verifies that components are enabled or disabled correctly based on the presence and order of enable/disable attributes.
    /// Specifically, ensures that when a component is disabled via an attribute, it is not composed, and when re-enabled by another component, both are composed in the correct order.
    /// </summary>
    [Test]
    public void AttributesPriorities()
    {
        var register = MockRegister();
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        Type[] types = { typeof(Composer26) };
        DisableComposerAttribute[] enableDisableAttributes = { new(typeof(Composer26)) };
        var composers =
            new ComposerGraph(composition, types, enableDisableAttributes, Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();
        composers.Compose();
        Assert.AreEqual(0, Composed.Count); // 26 gone

        types = new[] { typeof(Composer26), typeof(Composer27) }; // 26 disabled by assembly attribute, enabled by 27
        composers = new ComposerGraph(composition, types, enableDisableAttributes, Mock.Of<ILogger<ComposerGraph>>());
        Composed.Clear();
        composers.Compose();
        Assert.AreEqual(2, Composed.Count); // both
        Assert.AreEqual(typeof(Composer26), Composed[0]);
        Assert.AreEqual(typeof(Composer27), Composed[1]);
    }

    /// <summary>
    /// Tests that all <see cref="IComposer"/> implementations in the Umbraco.Core and Umbraco.Web namespaces
    /// can be loaded, sorted, and reported correctly using the ComposerGraph. Outputs the composer report and sorted types to the console.
    /// This ensures that composer discovery and dependency sorting are functioning as expected.
    /// </summary>
    [Test]
    public void AllComposers()
    {
        var typeFinder = TestHelper.GetTypeFinder();
        var typeLoader = new TypeLoader(
            typeFinder,
            Mock.Of<ILogger<TypeLoader>>());

        var register = MockRegister();
        var builder = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        var allComposers = typeLoader.GetTypes<IComposer>().ToList();
        var types = allComposers
            .Where(x => x.FullName.StartsWith("Umbraco.Core.") || x.FullName.StartsWith("Umbraco.Web")).ToList();
        var composers = new ComposerGraph(builder, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());
        var requirements = composers.GetRequirements();
        var report = ComposerGraph.GetComposersReport(requirements);
        Console.WriteLine(report);
        var composerTypes = composers.SortComposers(requirements);

        foreach (var type in composerTypes)
        {
            Console.WriteLine(type);
        }
    }

    /// <summary>
    /// Provides a base class for composer-related unit tests in the Umbraco CMS test suite.
    /// Inherit from this class to set up and test custom composers in unit tests.
    /// </summary>
    public class TestComposerBase : IComposer
    {
    /// <summary>
    /// Adds the current test composer type to the list of composed components using the specified Umbraco builder.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> instance used for composition.</param>
        public virtual void Compose(IUmbracoBuilder builder) => Composed.Add(GetType());
    }

    /// <summary>
    /// Represents a test composer class used specifically for component-related unit tests in the Umbraco CMS.
    /// </summary>
    public class Composer1 : TestComposerBase
    {
    }

    /// <summary>
    /// Represents a test composer used within the component tests to configure or register components for testing purposes.
    /// This class is utilized to verify the behavior of component composition in the Umbraco CMS core.
    /// </summary>
    [ComposeAfter(typeof(Composer4))]
    public class Composer2 : TestComposerBase
    {
    }

    /// <summary>
    /// A test composer component used within the <see cref="ComponentTests"/> class for unit testing scenarios.
    /// </summary>
    public class Composer3 : TestComposerBase
    {
    }

    /// <summary>
    /// Represents a test composer class used within component tests to configure or register components for testing scenarios.
    /// This class is specific to the test suite for verifying component composition in Umbraco.
    /// </summary>
    public class Composer4 : TestComposerBase
    {
    }

    /// <summary>
    /// Unit test class for the Composer6 component.
    /// </summary>
    [Disable]
    public class Composer6 : TestComposerBase
    {
    }

    /// <summary>
    /// Unit test class for the Composer7 component in Umbraco.
    /// Contains tests to verify the behavior and integration of Composer7 within the component system.
    /// </summary>
    public class Composer7 : TestComposerBase
    {
    }

    /// <summary>
    /// Contains unit tests for the Composer8 component, verifying its behavior and integration within the Umbraco Core Components system.
    /// </summary>
    [Disable(typeof(Composer7))]
    [Enable(typeof(Composer6))]
    public class Composer8 : TestComposerBase
    {
    }

    /// <summary>
    /// Represents an interface for composing test components in unit tests.
    /// </summary>
    public interface ITestComposer : IComposer
    {
    }

    /// <summary>
    /// Unit test class for testing the behavior and integration of the Composer9 component within the Umbraco Core Components.
    /// </summary>
    [ComposeAfter(typeof(Composer2))]
    [ComposeAfter(typeof(Composer4))]
    public class Composer9 : TestComposerBase, ITestComposer
    {
    }

    /// <summary>
    /// Contains unit tests for the <c>Composer10</c> component, verifying its behavior and integration within the Umbraco Core Components.
    /// </summary>
    [ComposeAfter(typeof(ITestComposer))]
    public class Composer10 : TestComposerBase
    {
    }

    /// <summary>
    /// Represents a test class for the Composer11 component in the Umbraco CMS core components.
    /// Contains unit tests to verify the behavior and integration of Composer11.
    /// </summary>
    [ComposeAfter(typeof(ITestComposer), false)]
    public class Composer11 : TestComposerBase
    {
    }

    /// <summary>
    /// Represents a test component named Composer12, used within unit tests for the Umbraco Core Components.
    /// </summary>
    [ComposeAfter(typeof(Composer4), true)]
    public class Composer12 : TestComposerBase
    {
    }

    /// <summary>
    /// Unit test class for the <c>Composer13</c> component.
    /// </summary>
    [ComposeBefore(typeof(Composer1))]
    public class Composer13 : TestComposerBase
    {
    }

    /// <summary>
    /// Represents a resource used in component tests.
    /// </summary>
    public interface ISomeResource
    {
    }

    /// <summary>
    /// Represents a resource used specifically for component tests in the test suite.
    /// </summary>
    public class SomeResource : ISomeResource
    {
    }

    /// <summary>
    /// Unit tests for the <c>Composer20</c> component in Umbraco CMS.
    /// </summary>
    public class Composer20 : TestComposerBase
    {
    }

    /// <summary>
    /// Represents a test-specific component named Composer21, used within unit tests for component behavior in the Umbraco CMS core.
    /// </summary>
    [ComposeBefore(typeof(Composer20))]
    public class Composer21 : TestComposerBase
    {
    }

    /// <summary>
    /// Represents a test component (Composer22) used in unit tests for verifying component behavior.
    /// </summary>
    public class Composer22 : TestComposerBase
    {
    }

    /// <summary>
    /// Represents a test composer interface used for unit testing component composition in Umbraco.
    /// </summary>
    [ComposeAfter(typeof(Composer22))]
    public interface IComposer23 : IComposer
    {
    }

    /// <summary>
    /// Provides unit tests for the Composer24 component within the ComponentTests suite.
    /// This class is used to verify the behavior and integration of Composer24 in the Umbraco CMS core components.
    /// </summary>
    public class Composer24 : TestComposerBase, IComposer23
    {
    }

    // should insert itself between 22 and anything i23
    /// <summary>
    /// Contains unit tests for the Composer25 component within the Umbraco Core Components.
    /// This class is used to verify the behavior and integration of Composer25 in the system.
    /// </summary>
    [ComposeBefore(typeof(IComposer23))]
    ////[RequireComponent(typeof(Component22))] - not needed, implement i23
    public class Composer25 : TestComposerBase, IComposer23
    {
    }

    /// <summary>
    /// Represents a test component used in the ComponentTests to verify the behavior of Composer26 within the Umbraco Core Components.
    /// </summary>
    public class Composer26 : TestComposerBase
    {
    }

    /// <summary>
    /// Contains unit tests for the Composer27 component within the Umbraco Core Components.
    /// </summary>
    [Enable(typeof(Composer26))]
    public class Composer27 : TestComposerBase
    {
    }

    // TODO: move to Testing
    private static Type[] TypeArray<T1>() => new[] { typeof(T1) };

    private static Type[] TypeArray<T1, T2>() => new[] { typeof(T1), typeof(T2) };

    private static Type[] TypeArray<T1, T2, T3>() => new[] { typeof(T1), typeof(T2), typeof(T3) };

    private static Type[] TypeArray<T1, T2, T3, T4>() => new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };

    private static Type[] TypeArray<T1, T2, T3, T4, T5>() =>
        new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };

    private static Type[] TypeArray<T1, T2, T3, T4, T5, T6>() =>
        new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };

    private static Type[] TypeArray<T1, T2, T3, T4, T5, T6, T7>() => new[]
    {
        typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7),
    };

    private static Type[] TypeArray<T1, T2, T3, T4, T5, T6, T7, T8>() => new[]
    {
        typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
    };

    private static void AssertTypeArray(IReadOnlyList<Type> expected, IReadOnlyList<Type> test)
    {
        Assert.AreEqual(expected.Count, test.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.AreEqual(expected[i], test[i]);
        }
    }
}
