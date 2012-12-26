using Umbraco.Core.Persistence.Migrations.Syntax.Alter;
using Umbraco.Core.Persistence.Migrations.Syntax.Create;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;
using Umbraco.Core.Persistence.Migrations.Syntax.Insert;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename;
using Umbraco.Core.Persistence.Migrations.Syntax.Update;

namespace Umbraco.Core.Persistence.Migrations
{
    public abstract class MigrationBase : IMigration
    {
        internal IMigrationContext _context;

        public abstract void Up();
        public abstract void Down();

        public virtual void GetUpExpressions(IMigrationContext context)
        {
            _context = context;
            Up();
        }

        public virtual void GetDownExpressions(IMigrationContext context)
        {
            _context = context;
            Down();
        }

        public IAlterSyntaxBuilder Alter
        {
            get { return new AlterSyntaxBuilder(_context); }
        }

        public ICreateBuilder Create
        {
            get { return new CreateBuilder(_context); }
        }

        public IDeleteBuilder Delete
        {
            get { return new DeleteBuilder(_context); }
        }

        public IExecuteBuilder Execute
        {
            get { return new ExecuteBuilder(_context); }
        }

        public IInsertBuilder Insert
        {
            get { return new InsertBuilder(_context); }
        }

        public IRenameBuilder Rename
        {
            get { return new RenameBuilder(_context); }
        }

        public IUpdateBuilder Update
        {
            get { return new UpdateBuilder(_context); }
        }
    }
}