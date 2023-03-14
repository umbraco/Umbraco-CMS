using System.Reflection;
using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder;

/// <summary>
///     Manages API version handshake between client and server.
/// </summary>
public class ApiVersion
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiVersion" /> class.
    /// </summary>
    /// <param name="executingVersion">The currently executing version.</param>
    /// <exception cref="ArgumentNullException"></exception>
    internal ApiVersion(SemVersion executingVersion) =>
        Version = executingVersion ?? throw new ArgumentNullException(nameof(executingVersion));

    /// <summary>
    ///     Gets the currently executing API version.
    /// </summary>
    public static ApiVersion Current { get; }
        = new(CurrentAssemblyVersion);

    private static SemVersion CurrentAssemblyVersion
        => SemVersion.Parse(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion);

    /// <summary>
    ///     Gets the executing version of the API.
    /// </summary>
    public SemVersion Version { get; }
}
