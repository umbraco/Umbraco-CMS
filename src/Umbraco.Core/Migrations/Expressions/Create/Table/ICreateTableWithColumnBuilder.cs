namespace Umbraco.Core.Migrations.Expressions.Create.Table
{
    public interface ICreateTableWithColumnBuilder : IFluentBuilder
    {
        ICreateTableColumnAsTypeSyntax WithColumn(string name);
    }
}
