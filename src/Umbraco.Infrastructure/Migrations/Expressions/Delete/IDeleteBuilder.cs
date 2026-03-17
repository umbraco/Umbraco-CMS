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
    ///     Builds and executes an expression to delete keys and indexes from the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table from which to delete keys and indexes.</param>
    /// <param name="local">If true, deletes local (primary and unique) keys and indexes.</param>
    /// <param name="foreign">If true, deletes foreign keys and related indexes.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> to execute the delete operation.</returns>
    IExecutableBuilder KeysAndIndexes(string tableName, bool local = true, bool foreign = true);

    /// <summary>
    /// Specifies the column to be deleted in the current delete expression.
    /// </summary>
    /// <param name="columnName">The name of the column to delete.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.IDeleteColumnBuilder" /> to continue building the delete expression.</returns>
    IDeleteColumnBuilder Column(string columnName);

    /// <summary>
    /// Begins the definition of a delete operation for a foreign key constraint.
    /// </summary>
    /// <returns>An <see cref="IDeleteForeignKeyFromTableBuilder" /> to continue building the delete expression.</returns>
    IDeleteForeignKeyFromTableBuilder ForeignKey();

    /// <summary>
    ///     Specifies the foreign key to be deleted in the migration.
    /// </summary>
    /// <param name="foreignKeyName">The name of the foreign key to delete.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.IDeleteForeignKeyOnTableBuilder" /> that allows further configuration of the delete operation.</returns>
    IDeleteForeignKeyOnTableBuilder ForeignKey(string foreignKeyName);

    /// <summary>
    ///     Specifies the table to delete data from.
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    IDeleteDataBuilder FromTable(string tableName);

    /// <summary>
    /// Specifies the index to delete in the current table.
    /// </summary>
    /// <returns>An <see cref="IDeleteIndexForTableBuilder"/> for further configuration of the index deletion.</returns>
    IDeleteIndexForTableBuilder Index();

    /// <summary>
    ///     Specifies the name of the index to delete as part of a migration expression.
    /// </summary>
    /// <param name="indexName">The name of the index to delete.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.IDeleteIndexForTableBuilder" /> to continue building the delete expression.</returns>
    IDeleteIndexForTableBuilder Index(string indexName);

    /// <summary>
    /// Specifies the primary key to delete in the migration expression.
    /// </summary>
    /// <param name="primaryKeyName">The name of the primary key to be deleted.</param>
    /// <returns>An <see cref="IDeleteConstraintBuilder"/> for further constraint configuration.</returns>
    IDeleteConstraintBuilder PrimaryKey(string primaryKeyName);

    /// <summary>
    /// Specifies which unique constraint should be deleted from the database schema.
    /// </summary>
    /// <param name="constraintName">The name of the unique constraint to delete.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.IDeleteConstraintBuilder" /> to continue building the delete expression.</returns>
    IDeleteConstraintBuilder UniqueConstraint(string constraintName);

    /// <summary>
    ///     Begins building an expression to delete a default constraint from a table.
    /// </summary>
    /// <returns>
    /// An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.IDeleteDefaultConstraintOnTableBuilder" /> to continue building the delete expression.
    /// </returns>
    IDeleteDefaultConstraintOnTableBuilder DefaultConstraint();
}
