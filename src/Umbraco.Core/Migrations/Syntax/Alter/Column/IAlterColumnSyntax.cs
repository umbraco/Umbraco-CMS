namespace Umbraco.Core.Migrations.Syntax.Alter.Column
{
    public interface IAlterColumnSyntax : IFluentSyntax
    {
        IAlterColumnTypeSyntax OnTable(string name);
    }
}
