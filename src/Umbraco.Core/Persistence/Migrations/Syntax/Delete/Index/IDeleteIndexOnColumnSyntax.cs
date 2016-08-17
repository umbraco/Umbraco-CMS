using System;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Index
{
    public interface IDeleteIndexOnColumnSyntax : IFluentSyntax
    {
        [Obsolete("I don't think this would ever be used when dropping an index, see DeleteIndexExpression.ToString")]
        void OnColumn(string columnName);

        [Obsolete("I don't think this would ever be used when dropping an index, see DeleteIndexExpression.ToString")]
        void OnColumns(params string[] columnNames);
    }
}