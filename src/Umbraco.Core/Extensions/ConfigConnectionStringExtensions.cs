// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Extensions
{
    public static class ConfigConnectionStringExtensions
    {
        public static bool IsConnectionStringConfigured(this ConfigConnectionString databaseSettings)
            => databaseSettings != null &&
            !string.IsNullOrWhiteSpace(databaseSettings.ConnectionString) &&
            !string.IsNullOrWhiteSpace(databaseSettings.ProviderName);
    }
}
