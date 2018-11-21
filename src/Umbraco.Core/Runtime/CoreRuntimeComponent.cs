using System;
using System.Collections.Generic;
using System.Configuration;
using AutoMapper;
using LightInject;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.CompositionRoots;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.IO.MediaPathSchemes;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Core._Legacy.PackageActions;
using IntegerValidator = Umbraco.Core.PropertyEditors.Validators.IntegerValidator;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Runtime
{
    public class CoreRuntimeComponent : UmbracoComponentBase, IRuntimeComponent
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            var container = composition.Container;

            container.RegisterSingleton<IProfiler, LogProfiler>();
            container.RegisterSingleton<ProfilingLogger>();
            container.RegisterSingleton<IRuntimeState, RuntimeState>();

            container.RegisterFrom<ConfigurationCompositionRoot>();

            // register caches
            // need the deep clone runtime cache profiver to ensure entities are cached properly, ie
            // are cloned in and cloned out - no request-based cache here since no web-based context,
            // will be overriden later or
            container.RegisterSingleton(_ => new CacheHelper(
                new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider()),
                new StaticCacheProvider(),
                NullCacheProvider.Instance,
                new IsolatedRuntimeCache(type => new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider()))));
            container.RegisterSingleton(f => f.GetInstance<CacheHelper>().RuntimeCache);

            // register the plugin manager
            container.RegisterSingleton(f => new TypeLoader(f.GetInstance<IRuntimeCacheProvider>(), f.GetInstance<IGlobalSettings>(), f.GetInstance<ProfilingLogger>()));

            // register syntax providers - required by database factory
            container.Register<ISqlSyntaxProvider, MySqlSyntaxProvider>("MySqlSyntaxProvider");
            container.Register<ISqlSyntaxProvider, SqlCeSyntaxProvider>("SqlCeSyntaxProvider");
            container.Register<ISqlSyntaxProvider, SqlServerSyntaxProvider>("SqlServerSyntaxProvider");

            // register persistence mappers - required by database factory so needs to be done here
            // means the only place the collection can be modified is in a runtime - afterwards it
            // has been frozen and it is too late
            var mapperCollectionBuilder = container.RegisterCollectionBuilder<MapperCollectionBuilder>();
            ComposeMapperCollection(mapperCollectionBuilder);

            // register database factory - required to check for migrations
            // will be initialized with syntax providers and a logger, and will try to configure
            // from the default connection string name, if possible, else will remain non-configured
            // until properly configured (eg when installing)
            container.RegisterSingleton<IUmbracoDatabaseFactory, UmbracoDatabaseFactory>();
            container.RegisterSingleton(f => f.GetInstance<IUmbracoDatabaseFactory>().SqlContext);

            // register the scope provider
            container.RegisterSingleton<ScopeProvider>(); // implements both IScopeProvider and IScopeAccessor
            container.RegisterSingleton<IScopeProvider>(f => f.GetInstance<ScopeProvider>());
            container.RegisterSingleton<IScopeAccessor>(f => f.GetInstance<ScopeProvider>());

            // register MainDom
            container.RegisterSingleton<MainDom>();

            // register from roots
            container.RegisterFrom<RepositoryCompositionRoot>();
            container.RegisterFrom<ServicesCompositionRoot>();
            container.RegisterFrom<CoreMappingProfilesCompositionRoot>();

            // register database builder
            // *not* a singleton, don't want to keep it around
            container.Register<DatabaseBuilder>();

            // register filesystems
            container.RegisterSingleton<FileSystems>();
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().MediaFileSystem);
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().ScriptsFileSystem, Constants.Composing.FileSystems.ScriptFileSystem);
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().PartialViewsFileSystem, Constants.Composing.FileSystems.PartialViewFileSystem);
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().MacroPartialsFileSystem, Constants.Composing.FileSystems.PartialViewMacroFileSystem);
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().StylesheetsFileSystem, Constants.Composing.FileSystems.StylesheetFileSystem);
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().MasterPagesFileSystem, Constants.Composing.FileSystems.MasterpageFileSystem);
            container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().MvcViewsFileSystem, Constants.Composing.FileSystems.ViewFileSystem);

            // register manifest parser, will be injected in collection builders where needed
            container.RegisterSingleton<ManifestParser>();

            // register our predefined validators
            container.RegisterCollectionBuilder<ManifestValueValidatorCollectionBuilder>()
                .Add<RequiredValidator>()
                .Add<RegexValidator>()
                .Add<DelimitedValueValidator>()
                .Add<EmailValidator>()
                .Add<IntegerValidator>()
                .Add<DecimalValidator>();

            // properties and parameters derive from data editors
            container.RegisterCollectionBuilder<DataEditorCollectionBuilder>()
                .Add(factory => factory.GetInstance<TypeLoader>().GetDataEditors());
            container.RegisterSingleton<PropertyEditorCollection>();
            container.RegisterSingleton<ParameterEditorCollection>();

            // register a server registrar, by default it's the db registrar 
            container.RegisterSingleton<IServerRegistrar>(f =>
            {
                if ("true".InvariantEquals(ConfigurationManager.AppSettings["umbracoDisableElectionForSingleServer"]))
                    return new SingleServerRegistrar(f.GetInstance<IRuntimeState>());
                return new DatabaseServerRegistrar(
                    new Lazy<IServerRegistrationService>(f.GetInstance<IServerRegistrationService>),
                    new DatabaseServerRegistrarOptions());
            });

            // by default we'll use the database server messenger with default options (no callbacks),
            // this will be overridden by either the legacy thing or the db thing in the corresponding
            // components in the web project - fixme - should obsolete the legacy thing
            container.RegisterSingleton<IServerMessenger>(factory
                => new DatabaseServerMessenger(
                    factory.GetInstance<IRuntimeState>(),
                    factory.GetInstance<IScopeProvider>(),
                    factory.GetInstance<ISqlContext>(),
                    factory.GetInstance<ProfilingLogger>(),
                    factory.GetInstance<IGlobalSettings>(),
                    true, new DatabaseServerMessengerOptions()));

            container.RegisterCollectionBuilder<CacheRefresherCollectionBuilder>()
                .Add(factory => factory.GetInstance<TypeLoader>().GetCacheRefreshers());

            container.RegisterCollectionBuilder<PackageActionCollectionBuilder>()
                .Add(f => f.GetInstance<TypeLoader>().GetPackageActions());

            container.RegisterCollectionBuilder<PropertyValueConverterCollectionBuilder>()
                .Append(factory => factory.GetInstance<TypeLoader>().GetTypes<IPropertyValueConverter>());

            container.Register<IPublishedContentTypeFactory, PublishedContentTypeFactory>(new PerContainerLifetime());

            container.RegisterSingleton<IShortStringHelper>(factory
                => new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(factory.GetInstance<IUmbracoSettingsSection>())));

            container.RegisterCollectionBuilder<UrlSegmentProviderCollectionBuilder>()
                .Append<DefaultUrlSegmentProvider>();

            container.RegisterCollectionBuilder<PostMigrationCollectionBuilder>()
                .Add(factory => factory.GetInstance<TypeLoader>().GetTypes<IPostMigration>());

            container.RegisterSingleton<IMigrationBuilder, MigrationBuilder>();

            // by default, register a noop factory
            container.RegisterSingleton<IPublishedModelFactory, NoopPublishedModelFactory>();

            container.RegisterSingleton<IMediaPathScheme, TwoGuidsMediaPathScheme>();
        }

        protected virtual void ComposeMapperCollection(MapperCollectionBuilder builder)
        {
            builder.AddCore();
        }

        internal void Initialize(IEnumerable<Profile> mapperProfiles)
        {
            // mapper profiles have been registered & are created by the container
            Mapper.Initialize(configuration =>
            {
                foreach (var profile in mapperProfiles)
                    configuration.AddProfile(profile);
            });

            // ensure we have some essential directories
            // every other component can then initialize safely
            IOHelper.EnsurePathExists("~/App_Data");
            IOHelper.EnsurePathExists(SystemDirectories.Media);
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews);
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews + "/Partials");
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews + "/MacroPartials");
        }
    }
}
