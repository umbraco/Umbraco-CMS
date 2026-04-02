using System.ComponentModel;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Data;

/// <summary>
/// Provides a builder for creating delete data migration expressions.
/// </summary>
public class DeleteDataBuilder : ExpressionBuilderBase<DeleteDataExpression>,
    IDeleteDataBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Data.DeleteDataBuilder"/> class.
    /// </summary>
    /// <param name="expression">The <see cref="DeleteDataExpression"/> that defines the data to be deleted in the migration.</param>
    public DeleteDataBuilder(DeleteDataExpression expression)
        : base(expression)
    {
    }

    /// <inheritdoc />
    public IExecutableBuilder IsNull(string columnName)
    {
        Expression.Rows.Add(new DeletionDataDefinition { new(columnName, null) });
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
    public void Do() => Expression.Execute();

    private static DeletionDataDefinition GetData(object dataAsAnonymousType)
    {
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(dataAsAnonymousType);

        var data = new DeletionDataDefinition();
        foreach (PropertyDescriptor property in properties)
        {
            data.Add(new KeyValuePair<string, object?>(property.Name, property.GetValue(dataAsAnonymousType)));
        }

        return data;
    }
}
