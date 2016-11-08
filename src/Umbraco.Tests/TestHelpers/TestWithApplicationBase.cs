using System;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;
using Umbraco.Core.DI;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Events;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Plugins;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web.Services;
using UmbracoExamine;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides a base class for all Umbraco tests that require the Umbraco application.
    /// </summary>
    /// <remarks>
    /// <para>Sets the Umbraco application DI container.</para>
    /// <para>Defines the Compose method for DI composition.</para>
    /// <para>Sets all sorts of things such as logging, plugin manager, base services, database factory & context...</para>
    /// <para>Does *not* create a database.</para>
    /// </remarks>
    [TestFixture]
    [UmbracoTest(AutoMapper = true, ResetPluginManager = false)]
    public abstract class TestWithApplicationBase : TestWithSettingsBase
    {
        protected ILogger Logger => Container.GetInstance<ILogger>();

        protected IProfiler Profiler => Container.GetInstance<IProfiler>();

        protected ProfilingLogger ProfilingLogger => Container.GetInstance<ProfilingLogger>();

        protected CacheHelper CacheHelper => Container.GetInstance<CacheHelper>();

        protected virtual ISqlSyntaxProvider SqlSyntax => new SqlCeSyntaxProvider();

        protected IMapperCollection Mappers => Container.GetInstance<IMapperCollection>();

        /// <summary>
        /// Gets a value indicating whether the plugin manager should be resetted before and after each test.
        /// </summary>
        /// <remarks>
        /// False by default, so the plugin manager does not need to re-scan all of the assemblies and tests run faster.
        /// Can be overriden if the plugin manager does need to reset, usually when SetupPluginManager has been overriden.
        /// </remarks>
        protected virtual bool PluginManagerResetRequired => false;

        public override void SetUp()
        {
            base.SetUp();

            TestHelper.InitializeContentDirectories();

            // initialize legacy mapings for core editors
            // create the legacy prop-eds mapping
            if (LegacyPropertyEditorIdToAliasConverter.Count() == 0)
                LegacyPropertyEditorIdToAliasConverter.CreateMappingsForCoreEditors();
        }

        public override void TearDown()
        {
            base.TearDown();

            TestHelper.CleanContentDirectories();
            TestHelper.CleanUmbracoSettingsConfig();
        }

        protected override void Compose()
        {
            base.Compose();

            var settings = SettingsForTests.GetDefault();

            // default Datalayer/Repositories/SQL/Database/etc...
            Container.RegisterFrom<RepositoryCompositionRoot>();

            // register basic stuff that might need to be there for some container resolvers to work
            Container.RegisterSingleton(factory => SettingsForTests.GetDefault());
            Container.RegisterSingleton(factory => settings.Content);
            Container.RegisterSingleton(factory => settings.Templates);
            Container.Register<IServiceProvider, ActivatorServiceProvider>();
            Container.Register(factory => new MediaFileSystem(Mock.Of<IFileSystem>()));
            Container.RegisterSingleton<IExamineIndexCollectionAccessor, TestIndexCollectionAccessor>();

            // replace some stuff
            Container.RegisterSingleton(factory => Mock.Of<IFileSystem>(), "ScriptFileSystem");
            Container.RegisterSingleton(factory => Mock.Of<IFileSystem>(), "PartialViewFileSystem");
            Container.RegisterSingleton(factory => Mock.Of<IFileSystem>(), "PartialViewMacroFileSystem");
            Container.RegisterSingleton(factory => Mock.Of<IFileSystem>(), "StylesheetFileSystem");

            // need real file systems here as templates content is on-disk only
            //Container.RegisterSingleton<IFileSystem>(factory => Mock.Of<IFileSystem>(), "MasterpageFileSystem");
            //Container.RegisterSingleton<IFileSystem>(factory => Mock.Of<IFileSystem>(), "ViewFileSystem");
            Container.RegisterSingleton<IFileSystem>(factory => new PhysicalFileSystem("Views", "/views"), "ViewFileSystem");
            Container.RegisterSingleton<IFileSystem>(factory => new PhysicalFileSystem("MasterPages", "/masterpages"), "MasterpageFileSystem");

            // no factory (noop)
            Container.RegisterSingleton<IPublishedContentModelFactory, NoopPublishedContentModelFactory>();

            // register application stuff (database factory & context, services...)
            Container.RegisterCollectionBuilder<MapperCollectionBuilder>()
                .Add(f => f.GetInstance<PluginManager>().ResolveAssignedMapperTypes());

            Container.RegisterSingleton<IEventMessagesFactory>(_ => new TransientEventMessagesFactory());
            Container.RegisterSingleton<IUmbracoDatabaseAccessor, TestUmbracoDatabaseAccessor>();
            Container.RegisterSingleton<IDatabaseFactory>(f => new DefaultDatabaseFactory(
                Core.Configuration.GlobalSettings.UmbracoConnectionName,
                TestObjects.GetDefaultSqlSyntaxProviders(Logger),
                Logger, f.GetInstance<IUmbracoDatabaseAccessor>(),
                Mock.Of<IMapperCollection>()));
            Container.RegisterSingleton(f => new DatabaseContext(
                f.GetInstance<IDatabaseFactory>(),
                Logger,
                Mock.Of<IRuntimeState>(),
                Mock.Of<IMigrationEntryService>()));

            Container.RegisterCollectionBuilder<UrlSegmentProviderCollectionBuilder>(); // empty
            Container.Register(factory
                => TestObjects.GetDatabaseUnitOfWorkProvider(factory.GetInstance<ILogger>(), factory.TryGetInstance<IDatabaseFactory>(), factory.TryGetInstance<RepositoryFactory>()));

            Container.RegisterFrom<ServicesCompositionRoot>();
            // composition root is doing weird things, fix
            Container.RegisterSingleton<IApplicationTreeService, ApplicationTreeService>();
            Container.RegisterSingleton<ISectionService, SectionService>();

            // somehow property editor ends up wanting this
            Container.RegisterSingleton(f => new ManifestBuilder(
                f.GetInstance<IRuntimeCacheProvider>(),
                new ManifestParser(f.GetInstance<ILogger>(), new DirectoryInfo(IOHelper.MapPath("~/App_Plugins")), f.GetInstance<IRuntimeCacheProvider>())
            ));

            // note - don't register collections, use builders
            Container.RegisterCollectionBuilder<PropertyEditorCollectionBuilder>();
        }
    }
}