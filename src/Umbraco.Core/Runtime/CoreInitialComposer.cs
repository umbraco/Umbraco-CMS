using System;
using System.Configuration;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.CompositionExtensions;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PackageActions;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using IntegerValidator = Umbraco.Core.PropertyEditors.Validators.IntegerValidator;

namespace Umbraco.Core.Runtime
{
    // core's initial composer composes before all core composers
    [ComposeBefore(typeof(ICoreComposer))]
    public class CoreInitialComposer : ComponentComposer<CoreInitialComponent>
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            // composers
            composition
                .ComposeConfiguration()
                .ComposeRepositories()
                .ComposeServices()
                .ComposeCoreMappingProfiles()
                .ComposeFileSystems();

            // register persistence mappers - required by database factory so needs to be done here
            // means the only place the collection can be modified is in a runtime - afterwards it
            // has been frozen and it is too late
            composition.Mappers().AddCoreMappers();

            // register the scope provider
            composition.RegisterUnique<ScopeProvider>(); // implements both IScopeProvider and IScopeAccessor
            composition.RegisterUnique<IScopeProvider>(f => f.GetInstance<ScopeProvider>());
            composition.RegisterUnique<IScopeAccessor>(f => f.GetInstance<ScopeProvider>());

            // register database builder
            // *not* a singleton, don't want to keep it around
            composition.Register<DatabaseBuilder>();

            // register manifest parser, will be injected in collection builders where needed
            composition.RegisterUnique<ManifestParser>();

            // register our predefined validators
            composition.ManifestValueValidators()
                .Add<RequiredValidator>()
                .Add<RegexValidator>()
                .Add<DelimitedValueValidator>()
                .Add<EmailValidator>()
                .Add<IntegerValidator>()
                .Add<DecimalValidator>();

            // register the manifest filter collection builder (collection is empty by default)
            composition.ManifestFilters();

            // properties and parameters derive from data editors
            composition.DataEditors()
                .Add(() => composition.TypeLoader.GetDataEditors());
            composition.RegisterUnique<PropertyEditorCollection>();
            composition.RegisterUnique<ParameterEditorCollection>();

            // Used to determine if a datatype/editor should be storing/tracking
            // references to media item/s
            composition.DataValueReferenceFactories();

            // register a server registrar, by default it's the db registrar
            composition.RegisterUnique<IServerRegistrar>(f =>
            {
                // TODO: this is a hack, use proper configuration!
                // also: we still register the full IServerMessenger because
                // even on 1 single server we can have 2 concurrent app domains
                var singleServer = "true".InvariantEquals(ConfigurationManager.AppSettings[Constants.AppSettings.DisableElectionForSingleServer]);
                return singleServer
                    ? (IServerRegistrar) new SingleServerRegistrar(f.GetInstance<IRuntimeState>())
                    : new DatabaseServerRegistrar(
                        new Lazy<IServerRegistrationService>(f.GetInstance<IServerRegistrationService>),
                        new DatabaseServerRegistrarOptions());
            });

            // by default we'll use the database server messenger with default options (no callbacks),
            // this will be overridden by the db thing in the corresponding components in the web
            // project
            composition.RegisterUnique<IServerMessenger>(factory
                => new DatabaseServerMessenger(
                    factory.GetInstance<IRuntimeState>(),
                    factory.GetInstance<IScopeProvider>(),
                    factory.GetInstance<ISqlContext>(),
                    factory.GetInstance<IProfilingLogger>(),
                    factory.GetInstance<IGlobalSettings>(),
                    true, new DatabaseServerMessengerOptions()));

            composition.CacheRefreshers()
                .Add(() => composition.TypeLoader.GetCacheRefreshers());

            composition.PackageActions()
                .Add(() => composition.TypeLoader.GetPackageActions());

            composition.PropertyValueConverters()
                .Append(composition.TypeLoader.GetTypes<IPropertyValueConverter>());

            composition.RegisterUnique<IPublishedContentTypeFactory, PublishedContentTypeFactory>();

            composition.RegisterUnique<IShortStringHelper>(factory
                => new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(factory.GetInstance<IUmbracoSettingsSection>())));

            composition.UrlSegmentProviders()
                .Append<DefaultUrlSegmentProvider>();

            composition.RegisterUnique<IMigrationBuilder>(factory => new MigrationBuilder(factory));

            // by default, register a noop factory
            composition.RegisterUnique<IPublishedModelFactory, NoopPublishedModelFactory>();

            // by default, register a noop rebuilder
            composition.RegisterUnique<IPublishedSnapshotRebuilder, PublishedSnapshotRebuilder>();

            // will be injected in controllers when needed to invoke rest endpoints on Our
            composition.RegisterUnique<IInstallationService, InstallationService>();
            composition.RegisterUnique<IUpgradeService, UpgradeService>();
        }
    }
}
