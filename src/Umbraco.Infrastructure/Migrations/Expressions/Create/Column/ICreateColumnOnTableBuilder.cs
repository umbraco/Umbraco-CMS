using Umbraco.Core.Migrations.Expressions.Common;

namespace Umbraco.Core.Migrations.Expressions.Create.Column
{
    public interface ICreateColumnOnTableBuilder : IColumnTypeBuilder<ICreateColumnOptionBuilder>
    {
        /// <summary>
        /// Specifies the name of the table.
        /// </summary>
        ICreateColumnTypeBuilder OnTable(string name);
    }
}
