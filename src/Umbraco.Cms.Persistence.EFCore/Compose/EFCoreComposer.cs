using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.Compose;

public class EFCoreComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // builder.Services.AddEntityFrameworkSqlServer();
        // builder.Services.AddEntityFrameworkSqlite();
        builder.Services.AddDbContext<UmbracoEFContext>(options =>
        {
            //TODO make this possible to change
            string? connectionString = builder.Config.GetUmbracoConnectionString("umbracoDbDSN", out string? providerName);
            if (!connectionString.IsNullOrWhiteSpace())
            {
                if (providerName == "Microsoft.Data.Sqlite")
                {
                    options.UseSqlite(connectionString!);
                }
                else if (providerName == "Microsoft.Data.SqlClient")
                {
                    options.UseSqlServer(connectionString!);
                }
            }
        });
        builder.Services.AddUnique<IDatabaseInfo, EFDatabaseInfo>();
        builder.Services.AddUnique<IDatabaseSchemaCreatorFactory, EFDatabaseSchemaCreatorFactory>();
        builder.Services.AddUnique<IDatabaseDataCreator, EFCoreDatabaseDataCreator>();
        builder.Services.AddSingleton<UmbracoDbContextFactory>();
    }
}
