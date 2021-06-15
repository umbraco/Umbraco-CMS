using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Cms.Core.Configuration.Models
{
    public class RuntimeMinificationSettings
    {
        public bool UseInMemoryCache { get; set; } = false;

        /// <summary>
        /// The cache buster type to use
        /// </summary>
        public RuntimeMinificationCacheBuster CacheBuster { get; set; } = RuntimeMinificationCacheBuster.Version;
    }
}
