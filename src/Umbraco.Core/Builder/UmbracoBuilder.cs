using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.Core.Builder;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Umbraco.Web.Common.Builder
{
    public class UmbracoBuilder : IUmbracoBuilder
    {
        public IServiceCollection Services { get; }
        public IConfiguration Config { get; }
        public TypeLoader TypeLoader { get; }
        public ILoggerFactory BuilderLoggerFactory { get; }

        private readonly Dictionary<Type, ICollectionBuilder> _builders = new Dictionary<Type, ICollectionBuilder>();

        public UmbracoBuilder(IServiceCollection services, IConfiguration config, TypeLoader typeLoader)
            : this(services, config, typeLoader, NullLoggerFactory.Instance)
        { }

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
            var typeOfBuilder = typeof(TBuilder);

            if (_builders.TryGetValue(typeOfBuilder, out var o))
                return (TBuilder)o;

            var builder = new TBuilder();
            _builders[typeOfBuilder] = builder;
            return builder;
        }

        public void Build()
        {
            foreach (var builder in _builders.Values)
                builder.RegisterWith(Services);

            _builders.Clear();
        }

        private void AddCoreServices()
        {
            // TODO: Should this be an explicit public method accepting a service lifetime?
            // Register the aggregator and factory as transient.
            // Use TryAdd to allow simple refactoring to allow additional registrations.
            // Transiant registration matches the default registration of Mediatr and
            // should encourage the avoidance of singletons throughout the codebase.
            Services.TryAddTransient<ServiceFactory>(p => p.GetService);
            Services.TryAdd(new ServiceDescriptor(typeof(IEventAggregator), typeof(EventAggregator), ServiceLifetime.Transient));
        }
    }
}
