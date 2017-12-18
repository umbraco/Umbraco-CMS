namespace Umbraco.Core.Migrations.Syntax.Create.ForeignKey
{
    public interface ICreateForeignKeyPrimaryColumnSyntax : IFluentSyntax
    {
        ICreateForeignKeyCascadeSyntax PrimaryColumn(string column);
        ICreateForeignKeyCascadeSyntax PrimaryColumns(params string[] columns);
    }
}
