using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.DefaultConstraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    /// <summary>
    /// We are removing the cmsMacroPropertyType which the cmsMacroProperty references and the cmsMacroProperty.macroPropertyType column
    /// needs to be changed to editorAlias, we'll do this by removing the constraint,changing the macroPropertyType to the new 
    /// editorAlias column (and maintaing data so we can reference it)
    /// </summary>
    [Migration("7.0.0", 6, GlobalSettings.UmbracoMigrationName)]
    public class AlterCmsMacroPropertyTable : MigrationBase
    {
        public override void Up()
        {
            //now that the controlId column is renamed and now a string we need to convert
            if (Context == null || Context.Database == null) return;
            
            //var cpt = SqlSyntaxContext.SqlSyntaxProvider.GetConstraintsPerTable(Context.Database);
            //var di = SqlSyntaxContext.SqlSyntaxProvider.GetDefinedIndexes(Context.Database);

            if (Context.CurrentDatabaseProvider != DatabaseProviders.SqlServer)
            {
                Delete.DefaultConstraint().OnTable("cmsMacroProperty").OnColumn("macroPropertyHidden");
            }
            else
            {
                //If we are on SQLServer, we need to delete default constraints by name, older versions of umbraco did not name these default constraints
                // consistently so we need to look up the constraint name to delete, this only pertains to SQL Server and this issue:
                // http://issues.umbraco.org/issue/U4-4133
                var sqlServerSyntaxProvider = new SqlServerSyntaxProvider();
                var defaultConstraints = sqlServerSyntaxProvider.GetDefaultConstraintsPerColumn(Context.Database).Distinct();

                //lookup the constraint we want to delete, normally would be called "DF_cmsMacroProperty_macroPropertyHidden" but 
                // we cannot be sure with really old versions
                var constraint = defaultConstraints
                    .SingleOrDefault(x => x.Item1 == "cmsMacroProperty" && x.Item2 == "macroPropertyHidden");
                if (constraint != null)
                {
                    Execute.Sql(string.Format("ALTER TABLE [{0}] DROP CONSTRAINT [{1}]", "cmsMacroProperty", constraint.Item3));
                }
            }

            Delete.Column("macroPropertyHidden").FromTable("cmsMacroProperty");

            if (Context.CurrentDatabaseProvider == DatabaseProviders.MySql)
            {
                Delete.ForeignKey().FromTable("cmsMacroProperty").ForeignColumn("macroPropertyType").ToTable("cmsMacroPropertyType").PrimaryColumn("id");
            }
            else
            {
                //Before we try to delete this constraint, we'll see if it exists first, some older schemas never had it and some older schema's had this named
                // differently than the default.

                var keyConstraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct();
                var constraint = keyConstraints
                    .SingleOrDefault(x => x.Item1 == "cmsMacroProperty" && x.Item2 == "macroPropertyType" && x.Item3.InvariantStartsWith("PK_") == false);
                if (constraint != null)
                {
                    Delete.ForeignKey(constraint.Item3).OnTable("cmsMacroProperty");
                }
            }
            
            Alter.Table("cmsMacroProperty").AddColumn("editorAlias").AsString(255).NotNullable().WithDefaultValue("");

            //we need to get the data and create the migration scripts before we change the actual schema bits below!
            var list = Context.Database.Fetch<dynamic>("SELECT * FROM cmsMacroPropertyType");
            foreach (var item in list)
            {

                var alias = item.macroPropertyTypeAlias;
                //check if there's a map created 
                var newAlias = (string)LegacyParameterEditorAliasConverter.GetNewAliasFromLegacyAlias(alias);
                if (newAlias.IsNullOrWhiteSpace() == false)
                {
                    alias = newAlias;
                }

                //update the table with the alias, the current macroPropertyType will contain the original id
                Update.Table("cmsMacroProperty").Set(new { editorAlias = alias }).Where(new { macroPropertyType = item.id });
            }

            //drop the column now
            Delete.Column("macroPropertyType").FromTable("cmsMacroProperty");

            //drop the default constraint
            Delete.DefaultConstraint().OnTable("cmsMacroProperty").OnColumn("editorAlias");
        }

        public override void Down()
        {
            throw new DataLossException("Cannot downgrade from a version 7 database to a prior version, the database schema has already been modified");
        }
    }
}