namespace Umbraco.Core.Persistence.Migrations
{
    public abstract class BaseChange : IDatabaseChange
    {
        public abstract Sql ToSql();

        public abstract string TableName { get; }

        public abstract string Version { get; }
    }
}