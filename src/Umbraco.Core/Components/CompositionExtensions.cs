﻿using System;
using System.Runtime.CompilerServices;
using Umbraco.Core.Cache;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Composing;
using Umbraco.Core.Migrations;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Core._Legacy.PackageActions;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Provides extension methods to the <see cref="Composition"/> class.
    /// </summary>
    public static class CompositionExtensions
    {
        #region Collection Builders

        /// <summary>
        /// Gets the cache refreshers collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static CacheRefresherCollectionBuilder CacheRefreshers(this Composition composition)
            => composition.Container.GetService<CacheRefresherCollectionBuilder>();

        /// <summary>
        /// Gets the mappers collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static MapperCollectionBuilder Mappers(this Composition composition)
            => composition.Container.GetService<MapperCollectionBuilder>();

        /// <summary>
        /// Gets the package actions collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        internal static PackageActionCollectionBuilder PackageActions(this Composition composition)
            => composition.Container.GetService<PackageActionCollectionBuilder>();

        /// <summary>
        /// Gets the data editor collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static DataEditorCollectionBuilder DataEditors(this Composition composition)
            => composition.Container.GetService<DataEditorCollectionBuilder>();

        /// <summary>
        /// Gets the property value converters collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static PropertyValueConverterCollectionBuilder PropertyValueConverters(this Composition composition)
            => composition.Container.GetService<PropertyValueConverterCollectionBuilder>();

        /// <summary>
        /// Gets the url segment providers collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static UrlSegmentProviderCollectionBuilder UrlSegmentProviders(this Composition composition)
            => composition.Container.GetService<UrlSegmentProviderCollectionBuilder>();

        /// <summary>
        /// Gets the validators collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        internal static ManifestValueValidatorCollectionBuilder Validators(this Composition composition)
            => composition.Container.GetService<ManifestValueValidatorCollectionBuilder>();

        /// <summary>
        /// Gets the post-migrations collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        internal static PostMigrationCollectionBuilder PostMigrations(this Composition composition)
            => composition.Container.GetService<PostMigrationCollectionBuilder>();

        #endregion

        #region Singleton

        /// <summary>
        /// Sets the culture dictionary factory.
        /// </summary>
        /// <typeparam name="T">The type of the factory.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetCultureDictionaryFactory<T>(this Composition composition)
            where T : class, ICultureDictionaryFactory
        {
            composition.Services.AddSingleton<ICultureDictionaryFactory, T>();
        }

        /// <summary>
        /// Sets the culture dictionary factory.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a culture dictionary factory.</param>
        public static void SetCultureDictionaryFactory(this Composition composition, Func<IServiceProvider, ICultureDictionaryFactory> factory)
        {
            composition.Services.AddSingleton(factory);
        }

        /// <summary>
        /// Sets the culture dictionary factory.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A factory.</param>
        public static void SetCultureDictionaryFactory(this Composition composition, ICultureDictionaryFactory factory)
        {
            composition.Services.AddSingleton(_ => factory);
        }

        /// <summary>
        /// Sets the published content model factory.
        /// </summary>
        /// <typeparam name="T">The type of the factory.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetPublishedContentModelFactory<T>(this Composition composition)
            where T : class, IPublishedModelFactory
        {
            composition.Services.AddSingleton<IPublishedModelFactory, T>();
        }

        /// <summary>
        /// Sets the published content model factory.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a published content model factory.</param>
        public static void SetPublishedContentModelFactory(this Composition composition, Func<IServiceProvider, IPublishedModelFactory> factory)
        {
            composition.Services.AddSingleton(factory);
        }

        /// <summary>
        /// Sets the published content model factory.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A published content model factory.</param>
        public static void SetPublishedContentModelFactory(this Composition composition, IPublishedModelFactory factory)
        {
            composition.Services.AddSingleton(_ => factory);
        }

        /// <summary>
        /// Sets the server registrar.
        /// </summary>
        /// <typeparam name="T">The type of the server registrar.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetServerRegistrar<T>(this Composition composition)
            where T : class, IServerRegistrar
        {
            composition.Services.AddSingleton<IServerRegistrar, T>();
        }

        /// <summary>
        /// Sets the server registrar.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a server registar.</param>
        public static void SetServerRegistrar(this Composition composition, Func<IServiceProvider, IServerRegistrar> factory)
        {
            composition.Services.AddSingleton(factory);
        }

        /// <summary>
        /// Sets the server registrar.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="registrar">A server registrar.</param>
        public static void SetServerRegistrar(this Composition composition, IServerRegistrar registrar)
        {
            composition.Services.AddSingleton(_ => registrar);
        }

        /// <summary>
        /// Sets the server messenger.
        /// </summary>
        /// <typeparam name="T">The type of the server registrar.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetServerMessenger<T>(this Composition composition)
            where T : class, IServerMessenger
        {
            composition.Services.AddSingleton<IServerMessenger, T>();
        }

        /// <summary>
        /// Sets the server messenger.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a server messenger.</param>
        public static void SetServerMessenger(this Composition composition, Func<IServiceProvider, IServerMessenger> factory)
        {
            composition.Services.AddSingleton(factory);
        }

        /// <summary>
        /// Sets the server messenger.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="registrar">A server messenger.</param>
        public static void SetServerMessenger(this Composition composition, IServerMessenger registrar)
        {
            composition.Services.AddSingleton(_ => registrar);
        }

        /// <summary>
        /// Sets the short string helper.
        /// </summary>
        /// <typeparam name="T">The type of the short string helper.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetShortStringHelper<T>(this Composition composition)
            where T : class, IShortStringHelper
        {
            composition.Services.AddSingleton<IShortStringHelper, T>();
        }

        /// <summary>
        /// Sets the short string helper.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a short string helper.</param>
        public static void SetShortStringHelper(this Composition composition, Func<IServiceProvider, IShortStringHelper> factory)
        {
            composition.Services.AddSingleton(factory);
        }

        /// <summary>
        /// Sets the short string helper.
        /// </summary>
        /// <param name="composition">A composition.</param>
        /// <param name="helper">A short string helper.</param>
        public static void SetShortStringHelper(this Composition composition, IShortStringHelper helper)
        {
            composition.Services.AddSingleton(_ => helper);
        }

        #endregion
    }
}
