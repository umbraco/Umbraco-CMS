using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Migrations.Expressions.Delete.Expressions;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Delete.Data
{
    public class DeleteDataBuilder : ExpressionBuilderBase<DeleteDataExpression>,
        IDeleteDataBuilder
    {
        public DeleteDataBuilder(DeleteDataExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public IExecutableBuilder IsNull(string columnName)
        {
            Expression.Rows.Add(new DeletionDataDefinition { new KeyValuePair<string, object>(columnName, null) });
            return this;
        }

        /// <inheritdoc />
        public IDeleteDataBuilder Row(object dataAsAnonymousType)
        {
            Expression.Rows.Add(GetData(dataAsAnonymousType));
            return this;
        }

        /// <inheritdoc />
        public IExecutableBuilder AllRows()
        {
            Expression.IsAllRows = true;
            return this;
        }

        /// <inheritdoc />
        public void Do()
        {
            Expression.Execute();
        }

        private static DeletionDataDefinition GetData(object dataAsAnonymousType)
        {
            var properties = TypeDescriptor.GetProperties(dataAsAnonymousType);

            var data = new DeletionDataDefinition();
            foreach (PropertyDescriptor property in properties)
                data.Add(new KeyValuePair<string, object>(property.Name, property.GetValue(dataAsAnonymousType)));
            return data;
        }
    }
}
