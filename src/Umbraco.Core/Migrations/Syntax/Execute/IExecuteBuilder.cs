using System;

namespace Umbraco.Core.Migrations.Syntax.Execute
{
    public interface IExecuteBuilder : IFluentSyntax
    {
        void Sql(string sqlStatement);
        void Code(Func<IMigrationContext, string> codeStatement);
    }
}
