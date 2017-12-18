namespace Umbraco.Core.Migrations.Syntax.Delete.ForeignKey
{
    public interface IDeleteForeignKeyPrimaryColumnSyntax : IFluentSyntax
    {
        void PrimaryColumn(string column);
        void PrimaryColumns(params string[] columns);
    }
}
