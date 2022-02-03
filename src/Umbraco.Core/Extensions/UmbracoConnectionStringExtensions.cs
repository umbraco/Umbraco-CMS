// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Extensions
{
    public static class UmbracoConnectionStringExtensions
    {
        public static bool IsConnectionStringConfigured(this UmbracoConnectionString umbracoConnectionString)
            => umbracoConnectionString != null &&
            !string.IsNullOrWhiteSpace(umbracoConnectionString.ConnectionString) &&
            !string.IsNullOrWhiteSpace(umbracoConnectionString.ProviderName);
    }
}
