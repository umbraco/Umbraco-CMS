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
    ///     Builds a Create Table expression, and executes.
    /// </summary>
    IExecutableBuilder Table<TDto>(bool withoutKeysAndIndexes = false);

    /// <summary>
    ///     Builds a Create Keys and Indexes expression, and executes.
    /// </summary>
    IExecutableBuilder KeysAndIndexes<TDto>();

    /// <summary>
    ///     Builds a Create Keys and Indexes expression, and executes.
    /// </summary>
    IExecutableBuilder KeysAndIndexes(Type typeOfDto);

    /// <summary>
    ///     Builds a Create Table expression.
    /// </summary>
    ICreateTableWithColumnBuilder Table(string tableName);

    /// <summary>
    ///     Builds a Create Column expression.
    /// </summary>
    ICreateColumnOnTableBuilder Column(string columnName);

    /// <summary>
    ///     Builds a Create Foreign Key expression.
    /// </summary>
    ICreateForeignKeyFromTableBuilder ForeignKey();

    /// <summary>
    ///     Builds a Create Foreign Key expression.
    /// </summary>
    ICreateForeignKeyFromTableBuilder ForeignKey(string foreignKeyName);

    /// <summary>
    ///     Builds a Create Index expression.
    /// </summary>
    ICreateIndexForTableBuilder Index();

    /// <summary>
    ///     Builds a Create Index expression.
    /// </summary>
    ICreateIndexForTableBuilder Index(string indexName);

    /// <summary>
    ///     Builds a Create Primary Key expression.
    /// </summary>
    ICreateConstraintOnTableBuilder PrimaryKey();

    /// <summary>
    ///     Builds a Create Primary Key expression.
    /// </summary>
    ICreateConstraintOnTableBuilder PrimaryKey(string primaryKeyName);

    /// <summary>
    ///     Builds a Create Primary Key expression.
    /// </summary>
    ICreateConstraintOnTableBuilder PrimaryKey(bool clustered);

    /// <summary>
    ///     Builds a Create Primary Key expression.
    /// </summary>
    ICreateConstraintOnTableBuilder PrimaryKey(string primaryKeyName, bool clustered);

    /// <summary>
    ///     Builds a Create Unique Constraint expression.
    /// </summary>
    ICreateConstraintOnTableBuilder UniqueConstraint();

    /// <summary>
    ///     Builds a Create Unique Constraint expression.
    /// </summary>
    ICreateConstraintOnTableBuilder UniqueConstraint(string constraintName);

    /// <summary>
    ///     Builds a Create Constraint expression.
    /// </summary>
    ICreateConstraintOnTableBuilder Constraint(string constraintName);
}
