using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Migrations
{
    public interface IMigrationPlanExecutor
    {
        string Execute(MigrationPlan plan, string fromState);
    }
}
