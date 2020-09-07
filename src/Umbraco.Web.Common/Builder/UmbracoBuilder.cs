using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Umbraco.Web.Common.Builder
{
    public class UmbracoBuilder : IUmbracoBuilder
    {
        private readonly Dictionary<string, Action> _registrations = new Dictionary<string, Action>();

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

        public void Build()
        {
            foreach (var a in _registrations)
                a.Value();
        }
    }
}
