using Semver;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Migrations
{
    public interface IPostMigration : IDiscoverable
    {
        void Execute(string name, IScope scope, SemVersion originVersion, SemVersion targetVersion, ILogger logger);
    }
}
