﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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

namespace Umbraco.Core.Runtime
{
    public class CoreRuntimeComponent : UmbracoComponentBase, IRuntimeComponent
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            // register from roots
            composition.Container.RegisterFrom<RepositoryCompositionRoot>();
            composition.Container.RegisterFrom<ServicesCompositionRoot>();
            composition.Container.RegisterFrom<CoreMappingProfilesCompositionRoot>();

            // register database builder
            // *not* a singleton, don't want to keep it around
            composition.Container.Register<DatabaseBuilder>();

            // register filesystems
            composition.Container.RegisterSingleton<FileSystems>();
            composition.Container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().MediaFileSystem);
            composition.Container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().ScriptsFileSystem, Constants.Composing.FileSystems.ScriptFileSystem);
            composition.Container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().PartialViewsFileSystem, Constants.Composing.FileSystems.PartialViewFileSystem);
            composition.Container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().MacroPartialsFileSystem, Constants.Composing.FileSystems.PartialViewMacroFileSystem);
            composition.Container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().StylesheetsFileSystem, Constants.Composing.FileSystems.StylesheetFileSystem);
            composition.Container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().MasterPagesFileSystem, Constants.Composing.FileSystems.MasterpageFileSystem);
            composition.Container.RegisterSingleton(factory => factory.GetInstance<FileSystems>().MvcViewsFileSystem, Constants.Composing.FileSystems.ViewFileSystem);

            // register manifest parser, will be injected in collection builders where needed
            composition.Container.RegisterSingleton<ManifestParser>();

            // register our predefined validators
            composition.Container.RegisterCollectionBuilder<ManifestValueValidatorCollectionBuilder>()
                .Add<RequiredValidator>()
                .Add<RegexValidator>()
                .Add<DelimitedValueValidator>()
                .Add<EmailValidator>()
                .Add<IntegerValidator>()
                .Add<DecimalValidator>();

            // properties and parameters derive from data editors
            composition.Container.RegisterCollectionBuilder<DataEditorCollectionBuilder>()
                .Add(factory => factory.GetInstance<TypeLoader>().GetDataEditors());
            composition.Container.RegisterSingleton<PropertyEditorCollection>();
            composition.Container.RegisterSingleton<ParameterEditorCollection>();

            // register a server registrar, by default it's the db registrar 
            composition.Container.RegisterSingleton<IServerRegistrar>(f =>
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
            composition.Container.RegisterSingleton<IServerMessenger>(factory
                => new DatabaseServerMessenger(
                    factory.GetInstance<IRuntimeState>(),
                    factory.GetInstance<IScopeProvider>(),
                    factory.GetInstance<ISqlContext>(),
                    factory.GetInstance<ProfilingLogger>(),
                    factory.GetInstance<IGlobalSettings>(),
                    true, new DatabaseServerMessengerOptions()));

            composition.Container.RegisterCollectionBuilder<CacheRefresherCollectionBuilder>()
                .Add(factory => factory.GetInstance<TypeLoader>().GetCacheRefreshers());

            composition.Container.RegisterCollectionBuilder<PackageActionCollectionBuilder>()
                .Add(f => f.GetInstance<TypeLoader>().GetPackageActions());

            composition.Container.RegisterCollectionBuilder<PropertyValueConverterCollectionBuilder>()
                .Append(factory => factory.GetInstance<TypeLoader>().GetTypes<IPropertyValueConverter>());

            composition.Container.Register<IPublishedContentTypeFactory, PublishedContentTypeFactory>(new PerContainerLifetime());

            composition.Container.RegisterSingleton<IShortStringHelper>(factory
                => new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(factory.GetInstance<IUmbracoSettingsSection>())));

            composition.Container.RegisterCollectionBuilder<UrlSegmentProviderCollectionBuilder>()
                .Append<DefaultUrlSegmentProvider>();

            composition.Container.RegisterCollectionBuilder<PostMigrationCollectionBuilder>()
                .Add(factory => factory.GetInstance<TypeLoader>().GetTypes<IPostMigration>());

            composition.Container.RegisterSingleton<IMigrationBuilder, MigrationBuilder>();

            // by default, register a noop factory
            composition.Container.RegisterSingleton<IPublishedModelFactory, NoopPublishedModelFactory>();

            composition.Container.RegisterSingleton<IMediaPathScheme, TwoGuidsMediaPathScheme>();
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
