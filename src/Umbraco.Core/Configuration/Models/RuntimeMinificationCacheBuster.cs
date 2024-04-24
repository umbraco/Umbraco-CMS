namespace Umbraco.Cms.Core.Configuration.Models;

[Obsolete("Runtime minification is no longer supported. Will be removed entirely in V16.")]
public enum RuntimeMinificationCacheBuster
{
    Version,
    AppDomain,
    Timestamp,
}
