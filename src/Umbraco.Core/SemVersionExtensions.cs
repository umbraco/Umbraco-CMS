using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core
{
    public static class SemVersionExtensions
    {
        public static string ToSemanticString(this SemVersion semVersion)
        {
            return semVersion.ToString().Replace("--", "-").Replace("-+", "+");
        }
    }
}
