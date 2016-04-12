using System;
using NPoco;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Execute
{
    public interface IExecuteBuilder : IFluentSyntax
    {
        void Sql(string sqlStatement);
        void Code(Func<Database, string> codeStatement);
    }
}