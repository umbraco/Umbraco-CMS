using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Infrastructure.Migrations.PostMigrations;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0;

public class MigrateLogViewerQueriesFromFileToDb : MigrationBase
{
    internal static readonly IEnumerable<LogViewerQueryDto> _defaultLogQueries = new LogViewerQueryDto[]
    {
        new()
        {
            Name = "Find all logs where the Level is NOT Verbose and NOT Debug",
            Query = "Not(@Level='Verbose') and Not(@Level='Debug')",
        },
        new()
        {
            Name = "Find all logs that has an exception property (Warning, Error & Fatal with Exceptions)",
            Query = "Has(@Exception)",
        },
        new() { Name = "Find all logs that have the property 'Duration'", Query = "Has(Duration)" },
        new()
        {
            Name = "Find all logs that have the property 'Duration' and the duration is greater than 1000ms",
            Query = "Has(Duration) and Duration > 1000",
        },
        new()
        {
            Name = "Find all logs that are from the namespace 'Umbraco.Core'",
            Query = "StartsWith(SourceContext, 'Umbraco.Core')",
        },
        new()
        {
            Name = "Find all logs that use a specific log message template",
            Query = "@MessageTemplate = '[Timing {TimingId}] {EndMessage} ({TimingDuration}ms)'",
        },
        new()
        {
            Name = "Find logs where one of the items in the SortedComponentTypes property array is equal to",
            Query = "SortedComponentTypes[?] = 'Umbraco.Web.Search.ExamineComponent'",
        },
        new()
        {
            Name = "Find logs where one of the items in the SortedComponentTypes property array contains",
            Query = "Contains(SortedComponentTypes[?], 'DatabaseServer')",
        },
        new()
        {
            Name = "Find all logs that the message has localhost in it with SQL like",
            Query = "@Message like '%localhost%'",
        },
        new()
        {
            Name = "Find all logs that the message that starts with 'end' in it with SQL like",
            Query = "@Message like 'end%'"
        },
    };

    private readonly IHostingEnvironment _hostingEnvironment;

    public MigrateLogViewerQueriesFromFileToDb(IMigrationContext context, IHostingEnvironment hostingEnvironment)
        : base(context) =>
        _hostingEnvironment = hostingEnvironment;

    internal static string GetLogViewerQueryFile(IHostingEnvironment hostingEnvironment) =>
        hostingEnvironment.MapPathContentRoot(
            Path.Combine(Constants.SystemDirectories.Config, "logviewer.searches.config.js"));

    protected override void Migrate()
    {
        CreateDatabaseTable();
        MigrateFileContentToDB();
    }

    private void CreateDatabaseTable()
    {
        IEnumerable<string> tables = SqlSyntax.GetTablesInSchema(Context.Database);
        if (!tables.InvariantContains(Constants.DatabaseSchema.Tables.LogViewerQuery))
        {
            Create.Table<LogViewerQueryDto>().Do();
        }
    }

    private void MigrateFileContentToDB()
    {
        var logViewerQueryFile = GetLogViewerQueryFile(_hostingEnvironment);

        IEnumerable<LogViewerQueryDto>? logQueriesInFile = File.Exists(logViewerQueryFile)
            ? JsonConvert.DeserializeObject<LogViewerQueryDto[]>(File.ReadAllText(logViewerQueryFile))
            : _defaultLogQueries;

        LogViewerQueryDto[]? logQueriesInDb = Database.Query<LogViewerQueryDto>().ToArray();

        if (logQueriesInDb.Any())
        {
            return;
        }

        Database.InsertBulk(logQueriesInFile!);

        Context.AddPostMigration<DeleteLogViewerQueryFile>();
    }
}
