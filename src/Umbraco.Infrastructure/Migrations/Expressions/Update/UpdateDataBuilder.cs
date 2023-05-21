using System.ComponentModel;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Update.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Update;

public class UpdateDataBuilder : ExpressionBuilderBase<UpdateDataExpression>,
    IUpdateTableBuilder, IUpdateWhereBuilder, IExecutableBuilder
{
    public UpdateDataBuilder(UpdateDataExpression expression)
        : base(expression)
    {
    }

    /// <inheritdoc />
    public void Do() => Expression.Execute();

    /// <inheritdoc />
    public IUpdateWhereBuilder Set(object dataAsAnonymousType)
    {
        Expression.Set = GetData(dataAsAnonymousType);
        return this;
    }

    /// <inheritdoc />
    public IExecutableBuilder Where(object dataAsAnonymousType)
    {
        Expression.Where = GetData(dataAsAnonymousType);
        return this;
    }

    /// <inheritdoc />
    public IExecutableBuilder AllRows()
    {
        Expression.IsAllRows = true;
        return this;
    }

    private static List<KeyValuePair<string, object?>> GetData(object dataAsAnonymousType)
    {
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(dataAsAnonymousType);

        var data = new List<KeyValuePair<string, object?>>();
        foreach (PropertyDescriptor property in properties)
        {
            data.Add(new KeyValuePair<string, object?>(property.Name, property.GetValue(dataAsAnonymousType)));
        }

        return data;
    }
}
