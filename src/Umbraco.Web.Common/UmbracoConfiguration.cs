using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Web.Common.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common
{
    public class UmbracoConfiguration : IConfigurationRoot, IDisposable
    {
        private readonly IConfigurationRoot _inner;

        private UmbracoConfiguration(IWebHostEnvironment webHostEnvironment)
        {
            if (webHostEnvironment == null)
            {
                throw new ArgumentNullException(nameof(webHostEnvironment));
            }

            // The DataDirectory is used to resolve database file paths (directly supported by SQL CE and manually replaced for LocalDB)
            AppDomain.CurrentDomain.SetData("DataDirectory", webHostEnvironment.MapPathContentRoot(Core.Constants.SystemDirectories.Data));
        }

        public UmbracoConfiguration(IWebHostEnvironment webHostEnvironment, IConfigurationRoot inner)
            : this(webHostEnvironment)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public UmbracoConfiguration(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
            : this(webHostEnvironment)
        {
            if (configuration is not IConfigurationRoot root)
            {
                throw new ArgumentException($"This constructor is provided for convenience, {nameof(configuration)} must be an instance of IConfiguration root");
            }

            _inner = root;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => _inner.GetChildren();

        public IChangeToken GetReloadToken() => _inner.GetReloadToken();

        public IConfigurationSection GetSection(string key) => new ConfigurationSection(this, key);

        public string this[string key]
        {
            get => GetTransformed(key);
            set => _inner[key] = value;
        }

        public string GetTransformed(string key)
        {
            var value = _inner[key];

            if (string.IsNullOrEmpty(value) || !key.StartsWith("ConnectionStrings"))
            {
                return value;
            }

            return value.Replace("|DataDirectory|", $"{AppDomain.CurrentDomain.GetData("DataDirectory")}");
        }

        public void Reload() => _inner.Reload();

        public IEnumerable<IConfigurationProvider> Providers => _inner.Providers;

        public void Dispose() => _inner.DisposeIfDisposable();
    }
}
