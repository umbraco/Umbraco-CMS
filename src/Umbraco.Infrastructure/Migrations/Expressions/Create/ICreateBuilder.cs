using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Constraint;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create;

/// <summary>
///     Builds a Create expression.
/// </summary>
public interface ICreateBuilder : IFluentBuilder
{
    /// <summary>
    ///     Builds a builder for a Create Table expression for the specified DTO type.
    /// </summary>
    /// <param name="withoutKeysAndIndexes">If true, creates the table without keys and indexes.</param>
    /// <typeparam name="TDto">The type representing the table schema.</typeparam>
    /// <returns>An <see cref="IExecutableBuilder"/> to execute the create table expression.</returns>
    IExecutableBuilder Table<TDto>(bool withoutKeysAndIndexes = false);

    /// <summary>
    ///     Builds a Create Keys and Indexes expression, and executes.
    /// </summary>
    IExecutableBuilder KeysAndIndexes<TDto>();

    /// <summary>
    ///     Builds and executes an expression to create keys and indexes for the specified DTO type.
    /// </summary>
    /// <param name="typeOfDto">The <see cref="Type"/> of the DTO for which to create keys and indexes.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> to execute the create keys and indexes expression.</returns>
    IExecutableBuilder KeysAndIndexes(Type typeOfDto);

    /// <summary>
    ///     Begins building a CREATE TABLE expression for the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table to create.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ICreateTableWithColumnBuilder"/> to define columns for the new table.</returns>
    ICreateTableWithColumnBuilder Table(string tableName);

    /// <summary>
    ///     Begins building an expression to create a new column in a database table.
    /// </summary>
    /// <param name="columnName">The name of the column to create.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ICreateColumnOnTableBuilder" /> to continue building the column definition on the table.</returns>
    ICreateColumnOnTableBuilder Column(string columnName);

    /// <summary>
    /// Begins building a 'Create Foreign Key' expression for a database migration.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ICreateForeignKeyFromTableBuilder" /> that allows further configuration of the foreign key.</returns>
    ICreateForeignKeyFromTableBuilder ForeignKey();

    /// <summary>
    /// Begins building a 'Create Foreign Key' expression for a database migration with the specified foreign key name.
    /// </summary>
    /// <param name="foreignKeyName">The name of the foreign key to create.</param>
    /// <returns>
    /// An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ICreateForeignKeyFromTableBuilder" /> that allows further configuration of the foreign key.
    /// </returns>
    ICreateForeignKeyFromTableBuilder ForeignKey(string foreignKeyName);

    /// <summary>
    /// Creates and returns a builder for constructing a Create Index expression.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ICreateIndexForTableBuilder" /> used to further define the index creation.</returns>
    ICreateIndexForTableBuilder Index();

    /// <summary>
    /// Creates and returns a builder for constructing a Create Index expression with the specified name.
    /// </summary>
    /// <param name="indexName">The name of the index to create.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ICreateIndexForTableBuilder" /> used to further define the index creation.</returns>
    ICreateIndexForTableBuilder Index(string indexName);

    /// <summary>
    ///     Builds a Create Primary Key expression.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ICreateConstraintOnTableBuilder" /> to further define the primary key constraint.</returns>
    ICreateConstraintOnTableBuilder PrimaryKey();

    /// <summary>
    ///     Begins building an expression to create a primary key constraint with the specified name.
    /// </summary>
    /// <param name="primaryKeyName">The name of the primary key constraint to create.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ICreateConstraintOnTableBuilder" /> for further configuration of the primary key constraint.</returns>
    ICreateConstraintOnTableBuilder PrimaryKey(string primaryKeyName);

    /// <summary>
    ///     Builds a create primary key expression with the option to specify whether it is clustered.
    /// </summary>
    /// <param name="clustered">Indicates whether the primary key should be clustered.</param>
    /// <returns>An <see cref="ICreateConstraintOnTableBuilder"/> to further define the primary key constraint.</returns>
    ICreateConstraintOnTableBuilder PrimaryKey(bool clustered);

    /// <summary>
    ///     Builds a Create Primary Key expression.
    /// </summary>
    ICreateConstraintOnTableBuilder PrimaryKey(string primaryKeyName, bool clustered);

    /// <summary>
    ///     Builds a Create Unique Constraint expression.
    /// </summary>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ICreateConstraintOnTableBuilder" /> to further define the unique constraint.</returns>
    ICreateConstraintOnTableBuilder UniqueConstraint();

    /// <summary>
    ///     Begins building an expression to create a unique constraint in the database schema.
    /// </summary>
    /// <param name="constraintName">The name of the unique constraint to create.</param>
    /// <returns>An <see cref="ICreateConstraintOnTableBuilder"/> that can be used to further define the unique constraint, such as specifying the table and columns.</returns>
    ICreateConstraintOnTableBuilder UniqueConstraint(string constraintName);

    /// <summary>
    ///     Begins building a CREATE CONSTRAINT expression for the specified constraint name.
    /// </summary>
    /// <param name="constraintName">The name of the constraint to create.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ICreateConstraintOnTableBuilder" /> that can be used to further define the constraint.</returns>
    ICreateConstraintOnTableBuilder Constraint(string constraintName);
}
