using Semver;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Migrations
{
    public interface IPostMigration
    {
        void Execute(string name, IScope scope, SemVersion originVersion, SemVersion targetVersion, ILogger logger);
    }
}
