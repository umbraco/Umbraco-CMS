using System.ComponentModel;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert;

/// <summary>
///     Implements <see cref="IInsertIntoBuilder" />.
/// </summary>
public class InsertIntoBuilder : ExpressionBuilderBase<InsertDataExpression>,
    IInsertIntoBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InsertIntoBuilder"/> class.
    /// </summary>
    /// <param name="expression">An <see cref="InsertDataExpression"/> representing the data to insert.</param>
    public InsertIntoBuilder(InsertDataExpression expression)
        : base(expression)
    {
    }

    /// <inheritdoc />
    public void Do() => Expression.Execute();

    /// <inheritdoc />
    public IInsertIntoBuilder EnableIdentityInsert()
    {
        Expression.EnabledIdentityInsert = true;
        return this;
    }

    /// <inheritdoc />
    public IInsertIntoBuilder Row(object dataAsAnonymousType)
    {
        Expression.Rows.Add(GetData(dataAsAnonymousType));
        return this;
    }

    private static InsertionDataDefinition GetData(object dataAsAnonymousType)
    {
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(dataAsAnonymousType);

        var data = new InsertionDataDefinition();
        foreach (PropertyDescriptor property in properties)
        {
            data.Add(new KeyValuePair<string, object?>(property.Name, property.GetValue(dataAsAnonymousType)));
        }

        return data;
    }
}
