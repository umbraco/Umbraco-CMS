namespace Umbraco.Core.Migrations.Syntax.Delete.ForeignKey
{
    public interface IDeleteForeignKeyOnTableSyntax : IFluentSyntax
    {
        void OnTable(string foreignTableName);
    }
}
