using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete
{
    public class DeleteDataBuilder : IDeleteDataSyntax
    {
        private readonly DeleteDataExpression _expression;

        public DeleteDataBuilder(DeleteDataExpression expression)
        {
            _expression = expression;
        }

        public void IsNull(string columnName)
        {
            _expression.Rows.Add(new DeletionDataDefinition
                                     {
                                         new KeyValuePair<string, object>(columnName, null)
                                     });
        }

        public IDeleteDataSyntax Row(object dataAsAnonymousType)
        {
            _expression.Rows.Add(GetData(dataAsAnonymousType));
            return this;
        }

        public IDeleteDataSyntax InSchema(string schemaName)
        {
            _expression.SchemaName = schemaName;
            return this;
        }

        public void AllRows()
        {
            _expression.IsAllRows = true;
        }

        private static DeletionDataDefinition GetData(object dataAsAnonymousType)
        {
            var data = new DeletionDataDefinition();
            var properties = TypeDescriptor.GetProperties(dataAsAnonymousType);

            foreach (PropertyDescriptor property in properties)
            {
                data.Add(new KeyValuePair<string, object>(property.Name, property.GetValue(dataAsAnonymousType)));
            }

            return data;
        }
    }
}