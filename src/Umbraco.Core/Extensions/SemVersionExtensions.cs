// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Semver;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for <see cref="SemVersion"/>.
/// </summary>
public static class SemVersionExtensions
{
    /// <summary>
    /// Converts the semantic version to a properly formatted semantic version string.
    /// </summary>
    /// <param name="semVersion">The semantic version.</param>
    /// <returns>The semantic version string with corrected formatting.</returns>
    public static string ToSemanticString(this SemVersion semVersion) =>
        semVersion.ToString().Replace("--", "-").Replace("-+", "+");

    /// <summary>
    /// Converts the semantic version to a string without the build metadata.
    /// </summary>
    /// <param name="semVersion">The semantic version.</param>
    /// <returns>The semantic version string without build metadata.</returns>
    public static string ToSemanticStringWithoutBuild(this SemVersion semVersion)
    {
        var version = semVersion.ToSemanticString();
        var indexOfBuild = version.IndexOf('+');
        return indexOfBuild >= 0 ? version[..indexOfBuild] : version;
    }
}
