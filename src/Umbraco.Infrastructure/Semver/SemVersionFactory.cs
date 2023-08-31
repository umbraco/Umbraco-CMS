using System.Diagnostics.CodeAnalysis;
using Semver;
using Umbraco.Cms.Core.Semver;
using SemVersion = Umbraco.Cms.Core.Semver.SemVersion;

namespace Umbraco.Cms.Infrastructure.SemanticVersioning;

public class SemVersionFactory : ISemVersionFactory
{
    public SemVersion Parse(string informationalVersion)
    {
        var semver = Semver.SemVersion.Parse(informationalVersion, SemVersionStyles.Any);
        return new SemVersion(semver.Major, semver.Minor, semver.Patch, semver.Prerelease, semver.Metadata);
    }

    public bool TryParse(string informationalVersion, [MaybeNullWhen(false)] out SemVersion semVersion)
    {
        var result = Semver.SemVersion.TryParse(informationalVersion, SemVersionStyles.Any, out Semver.SemVersion? semver);

        semVersion = result ? new SemVersion(semver.Major, semver.Minor, semver.Patch, semver.Prerelease, semver.Metadata) : null;
        return result;

    }
}
