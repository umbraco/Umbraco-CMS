using System;
using System.Collections.Generic;
using System.IO;
using AutoMapper;
using LightInject;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.DI;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Plugins;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Core._Legacy.PackageActions;
using IntegerValidator = Umbraco.Core.PropertyEditors.IntegerValidator;

namespace Umbraco.Core
{
    public class CoreRuntimeComponent : UmbracoComponentBase, IRuntimeComponent
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            // register from roots
            composition.Container.RegisterFrom<ConfigurationCompositionRoot>();
            composition.Container.RegisterFrom<RepositoryCompositionRoot>();
            composition.Container.RegisterFrom<ServicesCompositionRoot>();
            composition.Container.RegisterFrom<CoreModelMappersCompositionRoot>();

            //TODO: Don't think we'll need this when the resolvers are all container resolvers
            composition.Container.RegisterSingleton<IServiceProvider, ActivatorServiceProvider>();

            // register filesystems
            composition.Container.Register<FileSystems>();
            composition.Container.Register(factory => factory.GetInstance<FileSystems>().MediaFileSystem);
            composition.Container.RegisterSingleton<IFileSystem>(factory => factory.GetInstance<FileSystems>().ScriptsFileSystem, "ScriptFileSystem");
            composition.Container.RegisterSingleton<IFileSystem>(factory => factory.GetInstance<FileSystems>().PartialViewsFileSystem, "PartialViewFileSystem");
            composition.Container.RegisterSingleton<IFileSystem>(factory => factory.GetInstance<FileSystems>().MacroPartialsFileSystem, "PartialViewMacroFileSystem");
            composition.Container.RegisterSingleton<IFileSystem>(factory => factory.GetInstance<FileSystems>().StylesheetsFileSystem, "StylesheetFileSystem");
            composition.Container.RegisterSingleton<IFileSystem>(factory => factory.GetInstance<FileSystems>().MasterPagesFileSystem, "MasterpageFileSystem");
            composition.Container.RegisterSingleton<IFileSystem>(factory => factory.GetInstance<FileSystems>().MvcViewsFileSystem, "ViewFileSystem");

            // register manifest builder, will be injected in eg PropertyEditorCollectionBuilder
            composition.Container.RegisterSingleton(factory
                => new ManifestParser(factory.GetInstance<ILogger>(), new DirectoryInfo(IOHelper.MapPath("~/App_Plugins")), factory.GetInstance<IRuntimeCacheProvider>()));
            composition.Container.RegisterSingleton<ManifestBuilder>();

            composition.Container.RegisterCollectionBuilder<PropertyEditorCollectionBuilder>()
                .Add(factory => factory.GetInstance<PluginManager>().ResolvePropertyEditors());

            composition.Container.RegisterCollectionBuilder<ParameterEditorCollectionBuilder>()
                .Add(factory => factory.GetInstance<PluginManager>().ResolveParameterEditors());

            // register our predefined validators
            composition.Container.RegisterCollectionBuilder<ValidatorCollectionBuilder>()
                .Add<RequiredManifestValueValidator>()
                .Add<RegexValidator>()
                .Add<DelimitedManifestValueValidator>()
                .Add<EmailValidator>()
                .Add<IntegerValidator>()
                .Add<DecimalValidator>();

            // register a server registrar, by default it's the db registrar unless the dev
            // has the legacy dist calls enabled - fixme - should obsolete the legacy thing
            composition.Container.RegisterSingleton(factory => UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled
                ? (IServerRegistrar)new ConfigServerRegistrar(UmbracoConfig.For.UmbracoSettings())
                : (IServerRegistrar)new DatabaseServerRegistrar(
                    new Lazy<IServerRegistrationService>(factory.GetInstance<IServerRegistrationService>),
                    new DatabaseServerRegistrarOptions()));

            // by default we'll use the database server messenger with default options (no callbacks),
            // this will be overridden by either the legacy thing or the db thing in the corresponding
            // components in the web project - fixme - should obsolete the legacy thing
            composition.Container.RegisterSingleton<IServerMessenger>(factory
                => new DatabaseServerMessenger(
                    factory.GetInstance<IRuntimeState>(),
                    factory.GetInstance<DatabaseContext>(),
                    factory.GetInstance<ILogger>(),
                    factory.GetInstance<ProfilingLogger>(),
                    true, new DatabaseServerMessengerOptions()));

            composition.Container.RegisterCollectionBuilder<CacheRefresherCollectionBuilder>()
                .Add(factory => factory.GetInstance<PluginManager>().ResolveCacheRefreshers());

            composition.Container.RegisterCollectionBuilder<PackageActionCollectionBuilder>()
                .Add(f => f.GetInstance<PluginManager>().ResolvePackageActions());

            composition.Container.RegisterCollectionBuilder<MigrationCollectionBuilder>()
                .Add(factory => factory.GetInstance<PluginManager>().ResolveTypes<IMigration>());

            // need to filter out the ones we dont want!! fixme - what does that mean?
            composition.Container.RegisterCollectionBuilder<PropertyValueConverterCollectionBuilder>()
                .Append(factory => factory.GetInstance<PluginManager>().ResolveTypes<IPropertyValueConverter>());

            composition.Container.RegisterSingleton<IShortStringHelper>(factory
                => new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(factory.GetInstance<IUmbracoSettingsSection>())));

            composition.Container.RegisterCollectionBuilder<UrlSegmentProviderCollectionBuilder>()
                .Append<DefaultUrlSegmentProvider>();

            // by default, register a noop factory
            composition.Container.RegisterSingleton<IPublishedContentModelFactory, NoopPublishedContentModelFactory>();
        }

        internal void Initialize(
            IEnumerable<ModelMapperConfiguration> modelMapperConfigurations)
        {
            //TODO: Remove these for v8!
            LegacyPropertyEditorIdToAliasConverter.CreateMappingsForCoreEditors();
            LegacyParameterEditorAliasConverter.CreateMappingsForCoreEditors();

            // model mapper configurations have been registered & are created by the container
            Mapper.Initialize(configuration =>
            {
                foreach (var m in modelMapperConfigurations)
                    m.ConfigureMappings(configuration);
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
