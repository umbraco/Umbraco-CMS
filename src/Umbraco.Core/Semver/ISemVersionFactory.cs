using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core.Semver;

public interface ISemVersionFactory
{
    SemVersion Create(int major = 1, int minor = 0, int patch = 0, string prerelease = "", string build = "") => new SemVersion(major, minor, patch, prerelease, build);
    SemVersion Parse(string informationalVersion);
    bool TryParse(string informationalVersion, [MaybeNullWhen(false)] out SemVersion semVersion);
}
