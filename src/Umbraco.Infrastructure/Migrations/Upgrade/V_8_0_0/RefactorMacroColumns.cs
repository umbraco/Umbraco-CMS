using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0
{
    public class RefactorMacroColumns : MigrationBase
    {
        public RefactorMacroColumns(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            if (ColumnExists(Cms.Core.Constants.DatabaseSchema.Tables.Macro, "macroXSLT"))
            {
                //special trick to add the column without constraints and return the sql to add them later
                AddColumn<MacroDto>("macroType", out var sqls1);
                AddColumn<MacroDto>("macroSource", out var sqls2);

                //populate the new columns with legacy data
                //when the macro type is PartialView, it corresponds to 7, else it is 4 for Unknown
                Execute.Sql($"UPDATE {Cms.Core.Constants.DatabaseSchema.Tables.Macro} SET macroSource = '', macroType = 4").Do();
                Execute.Sql($"UPDATE {Cms.Core.Constants.DatabaseSchema.Tables.Macro} SET macroSource = macroXSLT, macroType = 4 WHERE macroXSLT != '' AND macroXSLT IS NOT NULL").Do();
                Execute.Sql($"UPDATE {Cms.Core.Constants.DatabaseSchema.Tables.Macro} SET macroSource = macroScriptAssembly, macroType = 4 WHERE macroScriptAssembly != '' AND macroScriptAssembly IS NOT NULL").Do();
                Execute.Sql($"UPDATE {Cms.Core.Constants.DatabaseSchema.Tables.Macro} SET macroSource = macroScriptType, macroType = 4 WHERE macroScriptType != '' AND macroScriptType IS NOT NULL").Do();
                Execute.Sql($"UPDATE {Cms.Core.Constants.DatabaseSchema.Tables.Macro} SET macroSource = macroPython, macroType = 7 WHERE macroPython != '' AND macroPython IS NOT NULL").Do();

                //now apply constraints (NOT NULL) to new table
                foreach (var sql in sqls1) Execute.Sql(sql).Do();
                foreach (var sql in sqls2) Execute.Sql(sql).Do();

                //now remove these old columns
                Delete.Column("macroXSLT").FromTable(Cms.Core.Constants.DatabaseSchema.Tables.Macro).Do();
                Delete.Column("macroScriptAssembly").FromTable(Cms.Core.Constants.DatabaseSchema.Tables.Macro).Do();
                Delete.Column("macroScriptType").FromTable(Cms.Core.Constants.DatabaseSchema.Tables.Macro).Do();
                Delete.Column("macroPython").FromTable(Cms.Core.Constants.DatabaseSchema.Tables.Macro).Do();
            }
        }
    }
}
