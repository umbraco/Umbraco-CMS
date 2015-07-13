using Semver;

namespace Umbraco.Core
{
    public static class SemVersionExtensions
    {
        public static string ToSemanticString(this SemVersion semVersion)
        {
            return semVersion.ToString().Replace("--", "-").Replace("-+", "+");
        }
    }
}