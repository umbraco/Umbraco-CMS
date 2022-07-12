using System.Text;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Install;

/// <summary>
///     Represents ...
/// </summary>
public class DatabaseSchemaResult
{
    public DatabaseSchemaResult()
    {
        Errors = new List<Tuple<string, string>>();
        TableDefinitions = new List<TableDefinition>();
        ValidTables = new List<string>();
        ValidColumns = new List<string>();
        ValidConstraints = new List<string>();
        ValidIndexes = new List<string>();
        IndexDefinitions = new List<DbIndexDefinition>();
    }

    public List<Tuple<string, string>> Errors { get; }

    public List<TableDefinition> TableDefinitions { get; }

    public List<string> ValidTables { get; }

    // TODO: what are these exactly? TableDefinitions are those that should be there, IndexDefinitions are those that... are in DB?
    internal List<DbIndexDefinition> IndexDefinitions { get; }

    public List<string> ValidColumns { get; }

    public List<string> ValidConstraints { get; }

    public List<string> ValidIndexes { get; }

    /// <summary>
    ///     Determines whether the database contains an installed version.
    /// </summary>
    /// <remarks>
    ///     <para>A database contains an installed version when it contains at least one valid table.</para>
    /// </remarks>
    public bool DetermineHasInstalledVersion() => ValidTables.Count > 0;

    /// <summary>
    ///     Gets a summary of the schema validation result
    /// </summary>
    /// <returns>A string containing a human readable string with a summary message</returns>
    public string GetSummary()
    {
        var sb = new StringBuilder();
        if (Errors.Any() == false)
        {
            sb.AppendLine("The database schema validation didn't find any errors.");
            return sb.ToString();
        }

        // Table error summary
        if (Errors.Any(x => x.Item1.Equals("Table")))
        {
            sb.AppendLine("The following tables were found in the database, but are not in the current schema:");
            sb.AppendLine(string.Join(",", Errors.Where(x => x.Item1.Equals("Table")).Select(x => x.Item2)));
            sb.AppendLine(" ");
        }

        // Column error summary
        if (Errors.Any(x => x.Item1.Equals("Column")))
        {
            sb.AppendLine("The following columns were found in the database, but are not in the current schema:");
            sb.AppendLine(string.Join(",", Errors.Where(x => x.Item1.Equals("Column")).Select(x => x.Item2)));
            sb.AppendLine(" ");
        }

        // Constraint error summary
        if (Errors.Any(x => x.Item1.Equals("Constraint")))
        {
            sb.AppendLine(
                "The following constraints (Primary Keys, Foreign Keys and Indexes) were found in the database, but are not in the current schema:");
            sb.AppendLine(string.Join(",", Errors.Where(x => x.Item1.Equals("Constraint")).Select(x => x.Item2)));
            sb.AppendLine(" ");
        }

        // Index error summary
        if (Errors.Any(x => x.Item1.Equals("Index")))
        {
            sb.AppendLine("The following indexes were found in the database, but are not in the current schema:");
            sb.AppendLine(string.Join(",", Errors.Where(x => x.Item1.Equals("Index")).Select(x => x.Item2)));
            sb.AppendLine(" ");
        }

        // Unknown constraint error summary
        if (Errors.Any(x => x.Item1.Equals("Unknown")))
        {
            sb.AppendLine(
                "The following unknown constraints (Primary Keys, Foreign Keys and Indexes) were found in the database, but are not in the current schema:");
            sb.AppendLine(string.Join(",", Errors.Where(x => x.Item1.Equals("Unknown")).Select(x => x.Item2)));
            sb.AppendLine(" ");
        }

        return sb.ToString();
    }
}
