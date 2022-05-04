using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

public interface IColumnOptionBuilder<out TNext, out TNextFk> : IFluentBuilder
    where TNext : IFluentBuilder
    where TNextFk : IFluentBuilder
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

    TNextFk ForeignKey(string foreignKeyName, string primaryTableSchema, string primaryTableName,
        string primaryColumnName);

    TNextFk ForeignKey();

    TNextFk ReferencedBy(string foreignTableName, string foreignColumnName);

    TNextFk ReferencedBy(string foreignKeyName, string foreignTableName, string foreignColumnName);

    TNextFk ReferencedBy(string foreignKeyName, string foreignTableSchema, string foreignTableName,
        string foreignColumnName);
}
