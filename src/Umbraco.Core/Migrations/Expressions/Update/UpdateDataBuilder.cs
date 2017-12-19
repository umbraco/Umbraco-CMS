using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Umbraco.Core.Migrations.Expressions.Update.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Update
{
    /// <summary>
    /// Implements <see cref="IUpdateTableBuilder"/>, <see cref="IUpdateWhereBuilder"/>/
    /// </summary>
    public class UpdateDataBuilder : ExpressionBuilderBase<UpdateDataExpression>, IUpdateTableBuilder, IUpdateWhereBuilder
    {
        public UpdateDataBuilder(UpdateDataExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public IUpdateWhereBuilder Set(object dataAsAnonymousType)
        {
            Expression.Set = GetData(dataAsAnonymousType);
            return this;
        }

        /// <inheritdoc />
        public void Where(object dataAsAnonymousType)
        {
            Expression.Where = GetData(dataAsAnonymousType);
            Expression.Execute();
        }

        /// <inheritdoc />
        public void AllRows()
        {
            Expression.IsAllRows = true;
            Expression.Execute();
        }

        private static List<KeyValuePair<string, object>> GetData(object dataAsAnonymousType)
        {
            var properties = TypeDescriptor.GetProperties(dataAsAnonymousType);

            var data = new List<KeyValuePair<string, object>>();
            foreach (PropertyDescriptor property in properties)
                data.Add(new KeyValuePair<string, object>(property.Name, property.GetValue(dataAsAnonymousType)));
            return data;
        }
    }
}
