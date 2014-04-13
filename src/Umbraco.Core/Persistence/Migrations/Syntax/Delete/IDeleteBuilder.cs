﻿using System;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Column;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Constraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.DefaultConstraint;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.ForeignKey;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Index;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete
{
    public interface IDeleteBuilder : IFluentSyntax
    {
        void Table(string tableName);
        IDeleteColumnFromTableSyntax Column(string columnName);
        IDeleteForeignKeyFromTableSyntax ForeignKey();
        [Obsolete("Do not use this construct as it does not work with MySql, use the syntax: Delete.ForeignKey().FromTable(\"umbracoUser2app\").ForeignColumn(... instead")]
        IDeleteForeignKeyOnTableSyntax ForeignKey(string foreignKeyName);
        IDeleteDataSyntax FromTable(string tableName);
        IDeleteIndexForTableSyntax Index();
        IDeleteIndexForTableSyntax Index(string indexName);
        IDeleteConstraintOnTableSyntax PrimaryKey(string primaryKeyName);
        IDeleteConstraintOnTableSyntax UniqueConstraint(string constraintName);
        IDeleteDefaultConstraintOnTableSyntax DefaultConstraint();
    }
}