using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Migrations
{
    public interface IMigrationPlanExecutor
    {
        Task<string> ExecuteAsync(MigrationPlan plan, string fromState);
    }
}
