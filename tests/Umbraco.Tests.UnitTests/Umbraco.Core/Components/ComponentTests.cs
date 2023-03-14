// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
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

[TestFixture]
public class ComponentTests
{
    private static readonly List<Type> Composed = new();
    private static readonly IIOHelper IOHelper = TestHelper.IOHelper;
    private static readonly List<Type> Initialized = new();
    private static readonly List<Type> Terminated = new();

    private static IServiceProvider MockFactory(Action<Mock<IServiceProvider>> setup = null)
    {
        // FIXME: use IUmbracoDatabaseFactory vs UmbracoDatabaseFactory, clean it all up!
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
            Options.Create(new ContentSettings()));
        var eventAggregator = Mock.Of<IEventAggregator>();
        var scopeProvider = new ScopeProvider(
            new AmbientScopeStack(), new AmbientScopeContextStack(),Mock.Of<IDistributedLockingMechanismFactory>(),
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
        new VaryingRuntimeHash(),
        Mock.Of<IAppPolicyCache>(),
        new DirectoryInfo(TestHelper.GetHostingEnvironment().MapPathContentRoot(Constants.SystemDirectories.TempData)),
        Mock.Of<ILogger<TypeLoader>>(),
        Mock.Of<IProfiler>());

    [Test]
    public void Boot1A()
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

                if (type == typeof(Composer5))
                {
                    return new Composer5();
                }

                if (type == typeof(Component5))
                {
                    return new Component5(new SomeResource());
                }

                if (type == typeof(IProfilingLogger))
                {
                    return new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>());
                }

                if (type == typeof(ILogger<ComponentCollection>))
                {
                    return Mock.Of<ILogger<ComponentCollection>>();
                }

                throw new NotSupportedException(type.FullName);
            });
        });

        var builder = composition.WithCollectionBuilder<ComponentCollectionBuilder>();
        builder.RegisterWith(register);
        var components = builder.CreateCollection(factory);

        Assert.IsEmpty(components);
        components.Initialize();
        Assert.IsEmpty(Initialized);
        components.Terminate();
        Assert.IsEmpty(Terminated);
    }

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

    [Test]
    public void Initialize()
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

                if (type == typeof(Composer5))
                {
                    return new Composer5();
                }

                if (type == typeof(Composer5a))
                {
                    return new Composer5a();
                }

                if (type == typeof(Component5))
                {
                    return new Component5(new SomeResource());
                }

                if (type == typeof(Component5a))
                {
                    return new Component5a();
                }

                if (type == typeof(IProfilingLogger))
                {
                    return new ProfilingLogger(Mock.Of<ILogger<ProfilingLogger>>(), Mock.Of<IProfiler>());
                }

                if (type == typeof(ILogger<ComponentCollection>))
                {
                    return Mock.Of<ILogger<ComponentCollection>>();
                }

                throw new NotSupportedException(type.FullName);
            });
        });
        var composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        Type[] types = { typeof(Composer1), typeof(Composer5), typeof(Composer5a) };
        var composers = new ComposerGraph(composition, types, Enumerable.Empty<Attribute>(), Mock.Of<ILogger<ComposerGraph>>());

        Assert.IsEmpty(Composed);
        composers.Compose();
        AssertTypeArray(TypeArray<Composer1, Composer5, Composer5a>(), Composed);

        var builder = composition.WithCollectionBuilder<ComponentCollectionBuilder>();
        builder.RegisterWith(register);
        var components = builder.CreateCollection(factory);

        Assert.IsEmpty(Initialized);
        components.Initialize();
        AssertTypeArray(TypeArray<Component5, Component5a>(), Initialized);

        Assert.IsEmpty(Terminated);
        components.Terminate();
        AssertTypeArray(TypeArray<Component5a, Component5>(), Terminated);
    }

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

    [Test]
    public void AllComposers()
    {
        var typeFinder = TestHelper.GetTypeFinder();
        var typeLoader = new TypeLoader(
            typeFinder,
            new VaryingRuntimeHash(),
            AppCaches.Disabled.RuntimeCache,
            new DirectoryInfo(TestHelper.GetHostingEnvironment().MapPathContentRoot(Constants.SystemDirectories.TempData)),
            Mock.Of<ILogger<TypeLoader>>(),
            Mock.Of<IProfiler>());

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

    public class TestComposerBase : IComposer
    {
        public virtual void Compose(IUmbracoBuilder builder) => Composed.Add(GetType());
    }

    public class Composer1 : TestComposerBase
    {
    }

    [ComposeAfter(typeof(Composer4))]
    public class Composer2 : TestComposerBase
    {
    }

    public class Composer3 : TestComposerBase
    {
    }

    public class Composer4 : TestComposerBase
    {
    }

    public class Composer5 : TestComposerBase
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);
            builder.Components().Append<Component5>();
        }
    }

    [ComposeAfter(typeof(Composer5))]
    public class Composer5a : TestComposerBase
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);
            builder.Components().Append<Component5a>();
        }
    }

    public class TestComponentBase : IComponent
    {
        public virtual void Initialize() => Initialized.Add(GetType());

        public virtual void Terminate() => Terminated.Add(GetType());
    }

    public class Component5 : TestComponentBase
    {
        private readonly ISomeResource _resource;

        public Component5(ISomeResource resource) => _resource = resource;
    }

    public class Component5a : TestComponentBase
    {
    }

    [Disable]
    public class Composer6 : TestComposerBase
    {
    }

    public class Composer7 : TestComposerBase
    {
    }

    [Disable(typeof(Composer7))]
    [Enable(typeof(Composer6))]
    public class Composer8 : TestComposerBase
    {
    }

    public interface ITestComposer : IComposer
    {
    }

    [ComposeAfter(typeof(Composer2))]
    [ComposeAfter(typeof(Composer4))]
    public class Composer9 : TestComposerBase, ITestComposer
    {
    }

    [ComposeAfter(typeof(ITestComposer))]
    public class Composer10 : TestComposerBase
    {
    }

    [ComposeAfter(typeof(ITestComposer), false)]
    public class Composer11 : TestComposerBase
    {
    }

    [ComposeAfter(typeof(Composer4), true)]
    public class Composer12 : TestComposerBase
    {
    }

    [ComposeBefore(typeof(Composer1))]
    public class Composer13 : TestComposerBase
    {
    }

    public interface ISomeResource
    {
    }

    public class SomeResource : ISomeResource
    {
    }

    public class Composer20 : TestComposerBase
    {
    }

    [ComposeBefore(typeof(Composer20))]
    public class Composer21 : TestComposerBase
    {
    }

    public class Composer22 : TestComposerBase
    {
    }

    [ComposeAfter(typeof(Composer22))]
    public interface IComposer23 : IComposer
    {
    }

    public class Composer24 : TestComposerBase, IComposer23
    {
    }

    // should insert itself between 22 and anything i23
    [ComposeBefore(typeof(IComposer23))]
    ////[RequireComponent(typeof(Component22))] - not needed, implement i23
    public class Composer25 : TestComposerBase, IComposer23
    {
    }

    public class Composer26 : TestComposerBase
    {
    }

    [Enable(typeof(Composer26))]
    public class Composer27 : TestComposerBase
    {
    }

    // FIXME: move to Testing
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
