using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Configuration;

public interface IUmbracoVersion
{
    /// <summary>
    ///     Gets the non-semantic version of the Umbraco code.
    /// </summary>
    Version Version { get; }

    /// <summary>
    ///     Gets the semantic version comments of the Umbraco code.
    /// </summary>
    string Comment { get; }

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
    Version? AssemblyVersion { get; }

    /// <summary>
    ///     Gets the assembly file version of the Umbraco code.
    /// </summary>
    /// <remarks>
    ///     <para>The assembly version is the value of the <see cref="AssemblyFileVersionAttribute" />.</para>
    /// </remarks>
    Version? AssemblyFileVersion { get; }

    /// <summary>
    ///     Gets the semantic version of the Umbraco code.
    /// </summary>
    /// <remarks>
    ///     <para>The semantic version is the value of the <see cref="AssemblyInformationalVersionAttribute" />.</para>
    ///     <para>It is the full version of Umbraco, including comments.</para>
    /// </remarks>
    SemVersion SemanticVersion { get; }
}
