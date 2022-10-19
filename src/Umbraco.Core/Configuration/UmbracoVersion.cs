using System.Reflection;
using Umbraco.Cms.Core.Semver;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Configuration;

/// <summary>
///     Represents the version of the executing code.
/// </summary>
public class UmbracoVersion : IUmbracoVersion
{
    public UmbracoVersion()
    {
        Assembly umbracoCoreAssembly = typeof(SemVersion).Assembly;

        // gets the value indicated by the AssemblyVersion attribute
        AssemblyVersion = umbracoCoreAssembly.GetName().Version;

        // gets the value indicated by the AssemblyFileVersion attribute
        AssemblyFileVersion =
            Version.Parse(umbracoCoreAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ??
                          string.Empty);

        // gets the value indicated by the AssemblyInformationalVersion attribute
        // this is the true semantic version of the Umbraco Cms
        SemanticVersion =
            SemVersion.Parse(umbracoCoreAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? string.Empty);

        // gets the non-semantic version
        Version = SemanticVersion.GetVersion(3);
    }

    /// <summary>
    ///     Gets the non-semantic version of the Umbraco code.
    /// </summary>
    public Version Version { get; }

    /// <summary>
    ///     Gets the semantic version comments of the Umbraco code.
    /// </summary>
    public string Comment => SemanticVersion.Prerelease;

    /// <summary>
    ///     Gets the assembly version of the Umbraco code.
    /// </summary>
    /// <remarks>
    ///     <para>The assembly version is the value of the <see cref="AssemblyVersionAttribute" />.</para>
    ///     <para>
    ///         Is the one that the CLR checks for compatibility. Therefore, it changes only on
    ///         hard-breaking changes (for instance, on new major versions).
    ///     </para>
    /// </remarks>
    public Version? AssemblyVersion { get; }

    /// <summary>
    ///     Gets the assembly file version of the Umbraco code.
    /// </summary>
    /// <remarks>
    ///     <para>The assembly version is the value of the <see cref="AssemblyFileVersionAttribute" />.</para>
    /// </remarks>
    public Version? AssemblyFileVersion { get; }

    /// <summary>
    ///     Gets the semantic version of the Umbraco code.
    /// </summary>
    /// <remarks>
    ///     <para>The semantic version is the value of the <see cref="AssemblyInformationalVersionAttribute" />.</para>
    ///     <para>It is the full version of Umbraco, including comments.</para>
    /// </remarks>
    public SemVersion SemanticVersion { get; }
}
