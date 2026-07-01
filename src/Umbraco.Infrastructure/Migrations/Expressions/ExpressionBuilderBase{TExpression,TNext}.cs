using System.Data;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions;

/// <summary>
///     Provides a base class for expression builders.
/// </summary>
public abstract class ExpressionBuilderBase<TExpression, TNext> : ExpressionBuilderBase<TExpression>
    where TExpression : IMigrationExpression
    where TNext : IFluentBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExpressionBuilderBase{TExpression, TNext}" /> class.
    /// </summary>
    protected ExpressionBuilderBase(TExpression expression)
        : base(expression)
    {
    }

    private ColumnDefinition? Column => GetColumnForType();

    /// <summary>
    /// Retrieves the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.ColumnDefinition"/> associated with the current expression's type, if one exists.
    /// </summary>
    /// <returns>The corresponding <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.ColumnDefinition"/> if available; otherwise, <c>null</c>.</returns>
    public abstract ColumnDefinition? GetColumnForType();

    /// <summary>
    /// Sets the type of the column being configured to <see cref="DbType.AnsiString"/>, indicating a variable-length ANSI (non-Unicode) string.
    /// </summary>
    /// <returns>
    /// The next expression in the builder chain, allowing for fluent configuration.
    /// </returns>
    public TNext AsAnsiString()
    {
        if (Column is not null)
        {
            Column.Type = DbType.AnsiString;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the type of the column being configured to <see cref="DbType.AnsiString"/>, indicating a variable-length ANSI (non-Unicode) string.
    /// </summary>
    /// <returns>
    /// The next expression in the builder chain, allowing for fluent configuration.
    /// </returns>
    public TNext AsAnsiString(int size)
    {
        if (Column is not null)
        {
            Column.Type = DbType.AnsiString;
            Column.Size = size;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Specifies that the column type should be set to binary in the migration expression.
    /// </summary>
    /// <returns>The next expression builder in the fluent chain.</returns>
    public TNext AsBinary()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Binary;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Specifies that the column type should be set to binary with the specified size in the migration expression.
    /// </summary>
    /// <param name="size">The maximum size, in bytes, of the binary column.</param>
    /// <returns>The next expression builder in the fluent chain.</returns>
    public TNext AsBinary(int size)
    {
        if (Column is not null)
        {
            Column.Type = DbType.Binary;
            Column.Size = size;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the current column's data type to <see cref="DbType.Boolean"/>.
    /// </summary>
    /// <returns>The next expression builder in the fluent chain.</returns>
    public TNext AsBoolean()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Boolean;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the database column type to <see cref="DbType.Byte"/> for the current column in the migration expression.
    /// </summary>
    /// <returns>The next expression builder in the fluent migration chain.</returns>
    public TNext AsByte()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Byte;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the database column type to <see cref="DbType.Currency"/>, indicating that the column will store currency values.
    /// </summary>
    /// <returns>The next expression builder in the fluent chain.</returns>
    public TNext AsCurrency()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Currency;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the type of the current column to <see cref="System.Data.DbType.Date"/>, indicating that the column will store date values (without time).
    /// </summary>
    /// <returns>
    /// The next expression builder in the fluent migration chain, allowing further configuration.
    /// </returns>
    public TNext AsDate()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Date;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Specifies that the column type should be set to <see cref="DbType.DateTime"/>.
    /// </summary>
    /// <returns>The next expression builder in the fluent chain.</returns>
    public TNext AsDateTime()
    {
        if (Column is not null)
        {
            Column.Type = DbType.DateTime;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Specifies that the column type should be set to <c>decimal</c> in the database schema.
    /// </summary>
    /// <returns>The next expression builder in the fluent chain.</returns>
    public TNext AsDecimal()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Decimal;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Specifies that the column type should be set to <c>decimal</c> in the database schema with the specified precision and scale.
    /// </summary>
    /// <param name="size">The precision, i.e., the maximum total number of digits that the decimal column can store.</param>
    /// <param name="precision">The scale, i.e., the number of digits that can be stored to the right of the decimal point.</param>
    /// <returns>The next expression builder in the fluent chain.</returns>
    public TNext AsDecimal(int size, int precision)
    {
        if (Column is not null)
        {
            Column.Type = DbType.Decimal;
            Column.Size = size;
            Column.Precision = precision;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the type of the column to <see cref="System.Data.DbType.Double"/> in the migration expression.
    /// </summary>
    /// <returns>
    /// The next expression builder in the fluent migration chain.
    /// </returns>
    public TNext AsDouble()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Double;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Configures the column to use a fixed-length string data type with the specified size.
    /// </summary>
    /// <param name="size">The length of the fixed-length string column.</param>
/// <returns>The next expression builder in the fluent chain.</returns>
    public TNext AsFixedLengthString(int size)
    {
        if (Column is not null)
        {
            Column.Type = DbType.StringFixedLength;
            Column.Size = size;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the column type to a fixed length ANSI string with the specified size.
    /// </summary>
    /// <param name="size">The fixed length size of the ANSI string.</param>
    /// <returns>The next expression builder in the chain.</returns>
    public TNext AsFixedLengthAnsiString(int size)
    {
        if (Column is not null)
        {
            Column.Type = DbType.AnsiStringFixedLength;
            Column.Size = size;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the column type of the current expression to a floating-point number (corresponding to <see cref="DbType.Single"/>) and returns the next expression builder in the chain.
    /// </summary>
    /// <returns>The next expression builder in the fluent API chain.</returns>
    public TNext AsFloat()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Single;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the current column's database type to <see cref="System.Data.DbType.Guid"/>.
    /// </summary>
    /// <returns>The next expression builder in the fluent chain.</returns>
    public TNext AsGuid()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Guid;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Specifies that the column type should be <see cref="DbType.Int16"/> (16-bit integer).
    /// </summary>
    /// <returns>The next expression builder in the fluent chain.</returns>
    public TNext AsInt16()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Int16;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Specifies that the column type should be <see cref="DbType.Int32"/> (32-bit integer).
    /// </summary>
    /// <returns>The next expression builder in the fluent chain.</returns>
    public TNext AsInt32()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Int32;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the type of the current column to <see cref="DbType.Int64"/>, representing a 64-bit integer in the database schema.
    /// </summary>
    /// <returns>
    /// The next expression builder in the fluent migration chain.
    /// </returns>
    public TNext AsInt64()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Int64;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the type of the current column to <see cref="System.Data.DbType.String"/> in the migration expression.
    /// </summary>
    /// <returns>The next expression builder in the fluent migration chain.</returns>
    public TNext AsString()
    {
        if (Column is not null)
        {
            Column.Type = DbType.String;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the type of the current column to <see cref="System.Data.DbType.String"/> with the specified size in the migration expression.
    /// </summary>
    /// <param name="size">The maximum length of the string column.</param>
    /// <returns>The next expression builder in the fluent migration chain.</returns>
    public TNext AsString(int size)
    {
        if (Column is not null)
        {
            Column.Type = DbType.String;
            Column.Size = size;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the column type to <see cref="System.Data.DbType.Time"/>.
    /// </summary>
    /// <returns>The next expression builder in the chain.</returns>
    public TNext AsTime()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Time;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the column type to XML.
    /// </summary>
    /// <returns>The next expression builder in the chain.</returns>
    public TNext AsXml()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Xml;
        }

        return (TNext)(object)this;
    }

    public TNext AsXml(int size)
    {
        if (Column is not null)
        {
            Column.Type = DbType.Xml;
            Column.Size = size;
        }

        return (TNext)(object)this;
    }

    /// <summary>
    /// Sets the column to use a custom database type.
    /// </summary>
    /// <param name="customType">The custom database type to use for the column.</param>
    /// <returns>The next expression builder in the migration expression chain.</returns>
    public TNext AsCustom(string customType)
    {
        if (Column is not null)
        {
            Column.Type = null;
            Column.CustomType = customType;
        }

        return (TNext)(object)this;
    }
}
