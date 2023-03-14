using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Column;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Constraint;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Data;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.DefaultConstraint;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.ForeignKey;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Index;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete;

/// <summary>
///     Builds a Delete expression.
/// </summary>
public interface IDeleteBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table to delete.
    /// </summary>
    IExecutableBuilder Table(string tableName);

    /// <summary>
    ///     Builds a Delete Keys and Indexes expression, and executes.
    /// </summary>
    IExecutableBuilder KeysAndIndexes<TDto>(bool local = true, bool foreign = true);

    /// <summary>
    ///     Builds a Delete Keys and Indexes expression, and executes.
    /// </summary>
    IExecutableBuilder KeysAndIndexes(string tableName, bool local = true, bool foreign = true);

    /// <summary>
    ///     Specifies the column to delete.
    /// </summary>
    IDeleteColumnBuilder Column(string columnName);

    /// <summary>
    ///     Specifies the foreign key to delete.
    /// </summary>
    IDeleteForeignKeyFromTableBuilder ForeignKey();

    /// <summary>
    ///     Specifies the foreign key to delete.
    /// </summary>
    IDeleteForeignKeyOnTableBuilder ForeignKey(string foreignKeyName);

    /// <summary>
    ///     Specifies the table to delete data from.
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    IDeleteDataBuilder FromTable(string tableName);

    /// <summary>
    ///     Specifies the index to delete.
    /// </summary>
    IDeleteIndexForTableBuilder Index();

    /// <summary>
    ///     Specifies the index to delete.
    /// </summary>
    IDeleteIndexForTableBuilder Index(string indexName);

    /// <summary>
    ///     Specifies the primary key to delete.
    /// </summary>
    IDeleteConstraintBuilder PrimaryKey(string primaryKeyName);

    /// <summary>
    ///     Specifies the unique constraint to delete.
    /// </summary>
    IDeleteConstraintBuilder UniqueConstraint(string constraintName);

    /// <summary>
    ///     Specifies the default constraint to delete.
    /// </summary>
    IDeleteDefaultConstraintOnTableBuilder DefaultConstraint();
}
