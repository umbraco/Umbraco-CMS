namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete
{
    public interface IDeleteDataSyntax : IFluentSyntax
    {
        IDeleteDataSyntax Row(object dataAsAnonymousType);
        void AllRows();
        void IsNull(string columnName);
    }
}