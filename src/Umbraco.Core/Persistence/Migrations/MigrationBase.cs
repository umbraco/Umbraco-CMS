﻿using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter;
using Umbraco.Core.Persistence.Migrations.Syntax.Create;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;
using Umbraco.Core.Persistence.Migrations.Syntax.IfDatabase;
using Umbraco.Core.Persistence.Migrations.Syntax.Insert;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename;
using Umbraco.Core.Persistence.Migrations.Syntax.Update;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations
{
    public abstract class MigrationBase : IMigration
    {
        public ISqlSyntaxProvider SqlSyntax { get; private set; }
        public ILogger Logger { get; private set; }
        
        protected MigrationBase(ISqlSyntaxProvider sqlSyntax, ILogger logger)
        {
            SqlSyntax = sqlSyntax;
            Logger = logger;
        }

        public IMigrationContext Context;

        public abstract void Up();
        public abstract void Down();

        public virtual void GetUpExpressions(IMigrationContext context)
        {
            Context = context;
            Up();
        }

        public virtual void GetDownExpressions(IMigrationContext context)
        {
            Context = context;
            Down();
        }

        public IAlterSyntaxBuilder Alter
        {
            get { return new AlterSyntaxBuilder(Context, SqlSyntax); }
        }

        public ICreateBuilder Create
        {
            get { return new CreateBuilder(Context, SqlSyntax); }
        }

        public IDeleteBuilder Delete
        {
            get { return new DeleteBuilder(Context, SqlSyntax); }
        }

        public IExecuteBuilder Execute
        {
            get { return new ExecuteBuilder(Context, SqlSyntax); }
        }

        public IInsertBuilder Insert
        {
            get { return new InsertBuilder(Context, SqlSyntax); }
        }

        public IRenameBuilder Rename
        {
            get { return new RenameBuilder(Context, SqlSyntax); }
        }

        public IUpdateBuilder Update
        {
            get { return new UpdateBuilder(Context, SqlSyntax); }
        }

        public IIfDatabaseBuilder IfDatabase(params DatabaseProviders[] databaseProviders)
        {
            return new IfDatabaseBuilder(Context, SqlSyntax, databaseProviders);
        }
    }
}