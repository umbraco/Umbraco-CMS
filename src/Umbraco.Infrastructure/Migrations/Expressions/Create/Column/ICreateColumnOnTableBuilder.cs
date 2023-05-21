using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column;

public interface ICreateColumnOnTableBuilder : IColumnTypeBuilder<ICreateColumnOptionBuilder>
{
    /// <summary>
    ///     Specifies the name of the table.
    /// </summary>
    ICreateColumnTypeBuilder OnTable(string name);
}
