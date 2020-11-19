using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Umbraco.Core.Builder;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Common.Builder
{
    public class UmbracoBuilder : IUmbracoBuilder
    {
        private readonly Dictionary<Type, ICollectionBuilder> _builders = new Dictionary<Type, ICollectionBuilder>();

        public UmbracoBuilder(IServiceCollection services, IConfiguration config)
        {
            Services = services;
            Config = config;
        }

        public IServiceCollection Services { get; }
        public IConfiguration Config { get; }

        /// <remarks>
        /// TODO: Remove setter
        /// This should be a constructor parameter
        /// Attempting to fix it now opens a huge can of worms around logging setup
        /// &amp; use of IOptionsMoniker&lt;HostingSettings&gt; for AspNetCoreHostingEnvironment
        /// </remarks>
        public TypeLoader TypeLoader { get; set; }

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
    }
}
