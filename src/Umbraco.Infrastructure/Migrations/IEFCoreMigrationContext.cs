namespace Umbraco.Cms.Infrastructure.Migrations;

public interface IEFCoreMigrationContext
{
    /// <summary>
    /// Gets the current migration plan
    /// </summary>
    EFCoreMigrationPlan Plan { get; }
}
