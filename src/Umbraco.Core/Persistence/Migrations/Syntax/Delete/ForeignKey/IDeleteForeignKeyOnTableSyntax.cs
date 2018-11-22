namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.ForeignKey
{
    public interface IDeleteForeignKeyOnTableSyntax : IFluentSyntax
    {
        void OnTable(string foreignTableName);
    }
}