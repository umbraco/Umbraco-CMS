using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class RefactorMacroColumns : MigrationBase
    {
        public RefactorMacroColumns(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            if (ColumnExists(Constants.DatabaseSchema.Tables.Macro, "macroXSLT"))
            {
                //special trick to add the column without constraints and return the sql to add them later
                AddColumn<MacroDto>("macroType", out var sqls1);
                AddColumn<MacroDto>("macroSource", out var sqls2);

                //populate the new columns with legacy data
                Execute.Sql($"UPDATE {Constants.DatabaseSchema.Tables.Macro} SET macroSource = '', macroType = {(int)MacroTypes.Unknown}").Do();
                Execute.Sql($"UPDATE {Constants.DatabaseSchema.Tables.Macro} SET macroSource = macroXSLT, macroType = {(int)MacroTypes.Unknown} WHERE macroXSLT != '' AND macroXSLT IS NOT NULL").Do();
                Execute.Sql($"UPDATE {Constants.DatabaseSchema.Tables.Macro} SET macroSource = macroScriptAssembly, macroType = {(int)MacroTypes.Unknown} WHERE macroScriptAssembly != '' AND macroScriptAssembly IS NOT NULL").Do();
                Execute.Sql($"UPDATE {Constants.DatabaseSchema.Tables.Macro} SET macroSource = macroScriptType, macroType = {(int)MacroTypes.Unknown} WHERE macroScriptType != '' AND macroScriptType IS NOT NULL").Do();
                Execute.Sql($"UPDATE {Constants.DatabaseSchema.Tables.Macro} SET macroSource = macroPython, macroType = {(int)MacroTypes.PartialView} WHERE macroPython != '' AND macroPython IS NOT NULL").Do();

                //now apply constraints (NOT NULL) to new table
                foreach (var sql in sqls1) Execute.Sql(sql).Do();
                foreach (var sql in sqls2) Execute.Sql(sql).Do();

                //now remove these old columns
                Delete.Column("macroXSLT").FromTable(Constants.DatabaseSchema.Tables.Macro).Do();
                Delete.Column("macroScriptAssembly").FromTable(Constants.DatabaseSchema.Tables.Macro).Do();
                Delete.Column("macroScriptType").FromTable(Constants.DatabaseSchema.Tables.Macro).Do();
                Delete.Column("macroPython").FromTable(Constants.DatabaseSchema.Tables.Macro).Do();
            }
        }
    }
}
