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
            ValidIndexes = new List<string>();
        }

        public List<Tuple<string, string>> Errors { get; set; }

        public List<TableDefinition> TableDefinitions { get; set; }

        public List<string> ValidTables { get; set; }

        public List<string> ValidColumns { get; set; }

        public List<string> ValidConstraints { get; set; }

        public List<string> ValidIndexes { get; set; }

        internal IEnumerable<DbIndexDefinition> DbIndexDefinitions { get; set; }

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
            if (ValidTables.Count == 0)
                return new Version(0, 0, 0);

            //If Errors is empty or if TableDefinitions tables + columns correspond to valid tables + columns then we're at current version
            if (Errors.Any() == false ||
                (TableDefinitions.All(x => ValidTables.Contains(x.Name))
                 && TableDefinitions.SelectMany(definition => definition.Columns).All(x => ValidColumns.Contains(x.Name))))
                return UmbracoVersion.Current;

            //If Errors contains umbracoApp or umbracoAppTree its pre-6.0.0 -> new Version(4, 10, 0);
            if (Errors.Any(x => x.Item1.Equals("Table") && (x.Item2.InvariantEquals("umbracoApp") || x.Item2.InvariantEquals("umbracoAppTree"))))
            {
                //If Errors contains umbracoUser2app or umbracoAppTree foreignkey to umbracoApp exists its pre-4.8.0 -> new Version(4, 7, 0);
                if (Errors.Any(x =>
                               x.Item1.Equals("Constraint")
                               && (x.Item2.InvariantContains("umbracoUser2app_umbracoApp")
                                   || x.Item2.InvariantContains("umbracoAppTree_umbracoApp"))))
                {
                    return new Version(4, 7, 0);
                }

                return new Version(4, 8, 0);
            }

            //if the error is for umbracoServer
            if (Errors.Any(x => x.Item1.Equals("Table") && (x.Item2.InvariantEquals("umbracoServer"))))
            {
                return new Version(6, 0, 0);
            }

            //if the error indicates a problem with the column cmsMacroProperty.macroPropertyType then it is not version 7 
            // since these columns get removed in v7
            if (Errors.Any(x => x.Item1.Equals("Column") && (x.Item2.InvariantEquals("cmsMacroProperty,macroPropertyType"))))
            {
                //if the error is for this IX_umbracoNodeTrashed which is added in 6.2 AND in 7.1 but we do not have the above columns
                // then it must mean that we aren't on 6.2 so must be 6.1
                if (Errors.Any(x => x.Item1.Equals("Index") && (x.Item2.InvariantEquals("IX_umbracoNodeTrashed"))))
                {
                    return new Version(6, 1, 0);
                }
                else
                {
                    //if there are no errors for that index, then the person must have 6.2 installed
                    return new Version(6, 2, 0);
                }
            }

            //if the error indicates a problem with the constraint FK_cmsContent_cmsContentType_nodeId then it is not version 7.2 
            // since this gets added in 7.2.0 so it must be the previous version
            if (Errors.Any(x => x.Item1.Equals("Constraint") && (x.Item2.InvariantEquals("FK_cmsContent_cmsContentType_nodeId"))))
            {
                return new Version(7, 0, 0);
            }

            //if the error is for umbracoAccess it must be the previous version to 7.3 since that is when it is added
            if (Errors.Any(x => x.Item1.Equals("Table") && (x.Item2.InvariantEquals("umbracoAccess"))))
            {
                return new Version(7, 2, 5);
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
            //Index error summary
            if (Errors.Any(x => x.Item1.Equals("Index")))
            {
                sb.AppendLine("The following indexes were found in the database, but are not in the current schema:");
                sb.AppendLine(string.Join(",", Errors.Where(x => x.Item1.Equals("Index")).Select(x => x.Item2)));
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