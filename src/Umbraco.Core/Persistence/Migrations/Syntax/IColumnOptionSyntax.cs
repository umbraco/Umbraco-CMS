using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Migrations.Syntax
{
    public interface IColumnOptionSyntax<TNext, TNextFk> : IFluentSyntax
        where TNext : IFluentSyntax
        where TNextFk : IFluentSyntax
    {
        TNext WithDefault(SystemMethods method);
        TNext WithDefaultValue(object value);
        TNext Identity();
        TNext Indexed();
        TNext Indexed(string indexName);

        TNext PrimaryKey();
        TNext PrimaryKey(string primaryKeyName);
        TNext Nullable();
        TNext NotNullable();
        TNext Unique();
        TNext Unique(string indexName);

        TNextFk ForeignKey(string primaryTableName, string primaryColumnName);
        TNextFk ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName);
        TNextFk ForeignKey(string foreignKeyName, string primaryTableSchema, string primaryTableName, string primaryColumnName);
        TNextFk ForeignKey();

        TNextFk ReferencedBy(string foreignTableName, string foreignColumnName);
        TNextFk ReferencedBy(string foreignKeyName, string foreignTableName, string foreignColumnName);
        TNextFk ReferencedBy(string foreignKeyName, string foreignTableSchema, string foreignTableName, string foreignColumnName);
    }
}