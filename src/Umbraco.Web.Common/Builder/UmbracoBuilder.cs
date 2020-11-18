using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Common.Builder
{
    public class UmbracoBuilder : IUmbracoBuilder
    {
        private readonly Dictionary<string, Action> _registrations = new Dictionary<string, Action>();
        private readonly Dictionary<Type, ICollectionBuilder> _builders = new Dictionary<Type, ICollectionBuilder>();

        public UmbracoBuilder(IServiceCollection services, IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            Services = services;
            WebHostEnvironment = webHostEnvironment;
            Config = config;
        }

        public IServiceCollection Services { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }
        public IConfiguration Config { get; }        

        public IUmbracoBuilder AddWith(string key, Action add)
        {
            if (_registrations.ContainsKey(key)) return this;
            _registrations[key] = add;
            return this;
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
            foreach (var a in _registrations)
                a.Value();

            _registrations.Clear();

            foreach (var builder in _builders.Values)
                builder.RegisterWith(Services);

            _builders.Clear();
        }
    }
}
