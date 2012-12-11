using System;
using System.Collections.Generic;
using System.Data;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Defines an SqlSyntaxProvider
    /// </summary>
    public interface ISqlSyntaxProvider
    {
        string GetQuotedTableName(string tableName);
        string GetQuotedColumnName(string columnName);
        string GetQuotedName(string name);
        bool DoesTableExist(Database db, string tableName);
        string ToCreateTableStatement(TableDefinition tableDefinition);
        List<string> ToCreateForeignKeyStatements(TableDefinition tableDefinition);
        List<string> ToCreateIndexStatements(TableDefinition tableDefinition);
        DbType GetColumnDbType(Type valueType);
        string GetIndexType(IndexTypes indexTypes);
        string GetColumnDefinition(ColumnDefinition column, string tableName);
        string GetPrimaryKeyStatement(ColumnDefinition column, string tableName);
        string ToCreatePrimaryKeyStatement(TableDefinition table);
        string GetSpecialDbType(SpecialDbTypes dbTypes);
        string GetConstraintDefinition(ColumnDefinition column, string tableName);
        List<string> ToAlterIdentitySeedStatements(TableDefinition table);
        string CreateTable { get; }
        string DropTable { get; }
        string AddColumn { get; }
        string DropColumn { get; }
        string AlterColumn { get; }
        string RenameColumn { get; }
        string RenameTable { get; }
        string CreateSchema { get; }
        string AlterSchema { get; }
        string DropSchema { get; }
        string CreateIndex { get; }
        string DropIndex { get; }
        string InsertData { get; }
        string UpdateData { get; }
        string DeleteData { get; }
        string CreateConstraint { get; }
        string DeleteConstraint { get; }
        string CreateForeignKeyConstraint { get; }
        string Format(DatabaseModelDefinitions.ColumnDefinition column);
    }
}