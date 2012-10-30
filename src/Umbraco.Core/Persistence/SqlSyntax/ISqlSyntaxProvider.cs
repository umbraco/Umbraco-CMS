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
    }
}