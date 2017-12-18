namespace Umbraco.Core.Migrations.Syntax.Create.Column
{
    public interface ICreateColumnOnTableSyntax : IColumnTypeSyntax<ICreateColumnOptionSyntax>
    {
        ICreateColumnTypeSyntax OnTable(string name);
    }
}
