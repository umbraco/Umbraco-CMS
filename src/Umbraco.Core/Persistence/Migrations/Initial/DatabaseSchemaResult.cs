using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

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

            //If Errors is empty then we're at current version
            if (Errors.Any() == false)
                return UmbracoVersion.Current;

            //If Errors contains umbracoApp or umbracoAppTree its pre-6.0.0 -> new Version(4, 10, 0);
            if (
                Errors.Any(
                    x => x.Item1.Equals("Table") && (x.Item2.Equals("umbracoApp") || x.Item2.Equals("umbracoAppTree"))))
            {
                //If Errors contains umbracoUser2app or umbracoAppTree foreignkey to umbracoApp exists its pre-4.8.0 -> new Version(4, 7, 0);
                if (
                    Errors.Any(
                        x =>
                        x.Item1.Equals("Constraint") &&
                        (x.Item2.Contains("umbracoUser2app_umbracoApp") || x.Item2.Contains("umbracoAppTree_umbracoApp"))))
                {
                    return new Version(4, 7, 0);
                }

                return new Version(4, 10, 0);
            }

            return new Version(0, 0, 0);
        }

        /// <summary>
        /// Gets a summary of the schema validation result
        /// </summary>
        /// <returns>A string containing a human readable string with a summary message</returns>
        public string GetSummary()
        {
            return string.Empty;
        }
    }
}