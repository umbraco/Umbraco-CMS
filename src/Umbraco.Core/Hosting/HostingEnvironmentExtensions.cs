namespace Umbraco.Cms.Core.Hosting;

/// <summary>
/// Extension methods for <see cref="IHostingEnvironment" />.
/// </summary>
public static class HostingEnvironmentExtensions
{
    private static string? _backOfficePath;

    /// <summary>
    /// Gets the absolute URL path of the back office.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    /// <returns>The absolute URL path of the back office.</returns>
    public static string GetBackOfficePath(this IHostingEnvironment hostingEnvironment)
        // Store the resolved URL path to avoid repeated conversion
        => _backOfficePath ??= hostingEnvironment.ToAbsolute(Core.Constants.System.DefaultUmbracoPath);
}
