using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Migrations.Expressions.Delete.Expressions;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Delete.Data
{
    /// <summary>
    /// Implements <see cref="IDeleteDataBuilder"/>.
    /// </summary>
    public class DeleteDataBuilder : ExpressionBuilderBase<DeleteDataExpression>, IDeleteDataBuilder
    {
        public DeleteDataBuilder(DeleteDataExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public void IsNull(string columnName)
        {
            Expression.Rows.Add(new DeletionDataDefinition { new KeyValuePair<string, object>(columnName, null) });
        }

        /// <inheritdoc />
        public IDeleteDataBuilder Row(object dataAsAnonymousType)
        {
            Expression.Rows.Add(GetData(dataAsAnonymousType));
            return this;
        }

        /// <inheritdoc />
        public void AllRows()
        {
            Expression.IsAllRows = true;
            Expression.Execute();
        }

        /// <inheritdoc />
        public void Execute()
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
