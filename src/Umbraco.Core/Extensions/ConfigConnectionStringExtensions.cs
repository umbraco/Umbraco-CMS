// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Extensions
{
    public static class ConfigConnectionStringExtensions
    {
        public static bool IsConnectionStringConfigured(this IConfiguration databaseSettings)
            => databaseSettings != null &&
            !string.IsNullOrWhiteSpace(databaseSettings.GetConnectionString(Constants.System.UmbracoConnectionName)) &&
            !string.IsNullOrWhiteSpace(databaseSettings.GetConnectionString(Constants.System.UmbracoConnectionProviderName));
    }
}
