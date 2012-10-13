namespace Umbraco.Core.Persistence.Migrations
{
    public interface IDatabaseChange
    {
        string Version { get; }
        Sql ToSql();
    }
}