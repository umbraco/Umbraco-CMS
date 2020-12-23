// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;

namespace Umbraco.Core.DependencyInjection
{
    public class UmbracoBuilder : IUmbracoBuilder
    {
        private readonly Dictionary<Type, ICollectionBuilder> _builders = new Dictionary<Type, ICollectionBuilder>();

        public IServiceCollection Services { get; }

        public IConfiguration Config { get; }

        public TypeLoader TypeLoader { get; }

        public ILoggerFactory BuilderLoggerFactory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoBuilder"/> class.
        /// </summary>
        public UmbracoBuilder(IServiceCollection services, IConfiguration config, TypeLoader typeLoader)
            : this(services, config, typeLoader, NullLoggerFactory.Instance)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoBuilder"/> class.
        /// </summary>
        public UmbracoBuilder(IServiceCollection services, IConfiguration config, TypeLoader typeLoader, ILoggerFactory loggerFactory)
        {
            Services = services;
            Config = config;
            BuilderLoggerFactory = loggerFactory;
            TypeLoader = typeLoader;

            AddCoreServices();
        }

        /// <summary>
        /// Gets a collection builder (and registers the collection).
        /// </summary>
        /// <typeparam name="TBuilder">The type of the collection builder.</typeparam>
        /// <returns>The collection builder.</returns>
        public TBuilder WithCollectionBuilder<TBuilder>()
            where TBuilder : ICollectionBuilder, new()
        {
            Type typeOfBuilder = typeof(TBuilder);

            if (_builders.TryGetValue(typeOfBuilder, out ICollectionBuilder o))
            {
                return (TBuilder)o;
            }

            var builder = new TBuilder();
            _builders[typeOfBuilder] = builder;
            return builder;
        }

        public void Build()
        {
            foreach (ICollectionBuilder builder in _builders.Values)
            {
                builder.RegisterWith(Services);
            }

            _builders.Clear();
        }

        private void AddCoreServices()
        {
            // Register as singleton to allow injection everywhere.
            Services.AddSingleton<ServiceFactory>(p => p.GetService);
            Services.AddSingleton<IEventAggregator, EventAggregator>();
        }
    }
}
