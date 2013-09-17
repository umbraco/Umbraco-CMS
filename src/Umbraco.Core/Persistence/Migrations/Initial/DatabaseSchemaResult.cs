using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Initial
{
    public class DatabaseSchemaResult
    {
        public DatabaseSchemaResult()
        {
            Errors = new List<Tuple<string, string>>();
            TableDefinitions = new List<TableDefinition>();
            ValidTables = new List<string>();
            ValidColumns = new List<string>();
            ValidConstraints = new List<string>();
        }

        public List<Tuple<string, string>> Errors { get; set; }

        public List<TableDefinition> TableDefinitions { get; set; }

        public List<string> ValidTables { get; set; }

        public List<string> ValidColumns { get; set; }

        public List<string> ValidConstraints { get; set; }

        /// <summary>
        /// Determines the version of the currently installed database.
        /// </summary>
        /// <returns>
        /// A <see cref="Version"/> with Major and Minor values for 
        /// non-empty database, otherwise "0.0.0" for empty databases.
        /// </returns>
        public Version DetermineInstalledVersion()
        {
            //If (ValidTables.Count == 0) database is empty and we return -> new Version(0, 0, 0);
            if(ValidTables.Count == 0)
                return new Version(0, 0, 0);

            //If Errors is empty or if TableDefinitions tables + columns correspond to valid tables + columns then we're at current version
            if (!Errors.Any() ||
                (TableDefinitions.All(x => ValidTables.Contains(x.Name))
                 && TableDefinitions.SelectMany(definition => definition.Columns).All(x => ValidColumns.Contains(x.Name))))
                return UmbracoVersion.Current;

            //If Errors contains umbracoApp or umbracoAppTree its pre-6.0.0 -> new Version(4, 10, 0);
            if (Errors.Any(x => x.Item1.Equals("Table") && (x.Item2.Equals("umbracoApp") || x.Item2.Equals("umbracoAppTree"))))
            {
                //If Errors contains umbracoUser2app or umbracoAppTree foreignkey to umbracoApp exists its pre-4.8.0 -> new Version(4, 7, 0);
                if (Errors.Any(x =>
                               x.Item1.Equals("Constraint")
                               && (x.Item2.Contains("umbracoUser2app_umbracoApp")
                                   || x.Item2.Contains("umbracoAppTree_umbracoApp"))))
                {
                    return new Version(4, 7, 0);
                }

                return new Version(4, 9, 0);
            }
            
            //if the error is for umbracoServer
            if (Errors.Any(x => x.Item1.Equals("Table") && (x.Item2.Equals("umbracoServer"))))
            {
                return new Version(6, 0, 0);
            }

            //if the error indicates a problem with the column cmsDataType.controlId then it is not version 7 and the
            // last db change we made was the umbracoServer in 6.2
            if (Errors.Any(x => x.Item1.Equals("Column") && (x.Item2.Equals("cmsDataType,controlId"))))
            {
                return new Version(6, 1, 0);
            }

            return UmbracoVersion.Current;
        }

        /// <summary>
        /// Gets a summary of the schema validation result
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

            //Table error summary
            if (Errors.Any(x => x.Item1.Equals("Table")))
            {
                sb.AppendLine("The following tables were found in the database, but are not in the current schema:");
                sb.AppendLine(string.Join(",", Errors.Where(x => x.Item1.Equals("Table")).Select(x => x.Item2)));
                sb.AppendLine(" ");
            }
            //Column error summary
            if (Errors.Any(x => x.Item1.Equals("Column")))
            {
                sb.AppendLine("The following columns were found in the database, but are not in the current schema:");
                sb.AppendLine(string.Join(",", Errors.Where(x => x.Item1.Equals("Column")).Select(x => x.Item2)));
                sb.AppendLine(" ");
            }
            //Constraint error summary
            if (Errors.Any(x => x.Item1.Equals("Constraint")))
            {
                sb.AppendLine("The following constraints (Primary Keys, Foreign Keys and Indexes) were found in the database, but are not in the current schema:");
                sb.AppendLine(string.Join(",", Errors.Where(x => x.Item1.Equals("Constraint")).Select(x => x.Item2)));
                sb.AppendLine(" ");
            }
            //Unknown constraint error summary
            if (Errors.Any(x => x.Item1.Equals("Unknown")))
            {
                sb.AppendLine("The following unknown constraints (Primary Keys, Foreign Keys and Indexes) were found in the database, but are not in the current schema:");
                sb.AppendLine(string.Join(",", Errors.Where(x => x.Item1.Equals("Unknown")).Select(x => x.Item2)));
                sb.AppendLine(" ");
            }

            if (SqlSyntaxContext.SqlSyntaxProvider is MySqlSyntaxProvider)
            {
                sb.AppendLine("Please note that the constraints could not be validated because the current dataprovider is MySql.");
            }

            return sb.ToString();
        }
    }
}