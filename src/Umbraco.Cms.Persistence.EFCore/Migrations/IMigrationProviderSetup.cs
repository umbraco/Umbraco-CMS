using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Persistence.EFCore.Migrations;

public interface IMigrationProviderSetup
{
    string ProviderName { get; }
    void Setup(DbContextOptionsBuilder builder, string? connectionString);
}
