using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Persistence.Migrations.Syntax.Update.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Update
{
    public class UpdateDataBuilder : IUpdateSetSyntax, IUpdateWhereSyntax
    {
        private readonly UpdateDataExpression _expression;
        private readonly IMigrationContext _context;

        public UpdateDataBuilder(UpdateDataExpression expression, IMigrationContext context)
        {
            _context = context;
            _expression = expression;
        }

        public IUpdateWhereSyntax Set(object dataAsAnonymousType)
        {
            _expression.Set = GetData(dataAsAnonymousType);
            return this;
        }

        public void Where(object dataAsAnonymousType)
        {
            _expression.Where = GetData(dataAsAnonymousType);
        }

        public void AllRows()
        {
            _expression.IsAllRows = true;
        }

        private static List<KeyValuePair<string, object>> GetData(object dataAsAnonymousType)
        {
            var data = new List<KeyValuePair<string, object>>();
            var properties = TypeDescriptor.GetProperties(dataAsAnonymousType);

            foreach (PropertyDescriptor property in properties)
            {
                data.Add(new KeyValuePair<string, object>(property.Name, property.GetValue(dataAsAnonymousType)));
            }

            return data;
        }
    }
}