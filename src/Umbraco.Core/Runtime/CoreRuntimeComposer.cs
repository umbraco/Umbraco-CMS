using System;
using System.Configuration;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.Composers;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Core._Legacy.PackageActions;
using IntegerValidator = Umbraco.Core.PropertyEditors.Validators.IntegerValidator;

namespace Umbraco.Core.Runtime
{
    public class CoreRuntimeComposer : ComponentComposer<CoreRuntimeComponent>, IRuntimeComposer
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
            composition.WithCollectionBuilder<MapperCollectionBuilder>().AddCoreMappers();

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
            composition.WithCollectionBuilder<ManifestValueValidatorCollectionBuilder>()
                .Add<RequiredValidator>()
                .Add<RegexValidator>()
                .Add<DelimitedValueValidator>()
                .Add<EmailValidator>()
                .Add<IntegerValidator>()
                .Add<DecimalValidator>();

            // properties and parameters derive from data editors
            composition.WithCollectionBuilder<DataEditorCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetDataEditors());
            composition.RegisterUnique<PropertyEditorCollection>();
            composition.RegisterUnique<ParameterEditorCollection>();

            // register a server registrar, by default it's the db registrar
            composition.RegisterUnique<IServerRegistrar>(f =>
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
            composition.RegisterUnique<IServerMessenger>(factory
                => new DatabaseServerMessenger(
                    factory.GetInstance<IRuntimeState>(),
                    factory.GetInstance<IScopeProvider>(),
                    factory.GetInstance<ISqlContext>(),
                    factory.GetInstance<IProfilingLogger>(),
                    factory.GetInstance<IGlobalSettings>(),
                    true, new DatabaseServerMessengerOptions()));

            composition.WithCollectionBuilder<CacheRefresherCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetCacheRefreshers());

            composition.WithCollectionBuilder<PackageActionCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetPackageActions());

            composition.WithCollectionBuilder<PropertyValueConverterCollectionBuilder>()
                .Append(composition.TypeLoader.GetTypes<IPropertyValueConverter>());

            composition.RegisterUnique<IPublishedContentTypeFactory, PublishedContentTypeFactory>();

            composition.RegisterUnique<IShortStringHelper>(factory
                => new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(factory.GetInstance<IUmbracoSettingsSection>())));

            composition.WithCollectionBuilder<UrlSegmentProviderCollectionBuilder>()
                .Append<DefaultUrlSegmentProvider>();

            composition.WithCollectionBuilder<PostMigrationCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetTypes<IPostMigration>());

            composition.RegisterUnique<IMigrationBuilder>(factory => new MigrationBuilder(factory));

            // by default, register a noop factory
            composition.RegisterUnique<IPublishedModelFactory, NoopPublishedModelFactory>();
        }
    }
}
