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

    public abstract ColumnDefinition? GetColumnForType();

    public TNext AsAnsiString()
    {
        if (Column is not null)
        {
            Column.Type = DbType.AnsiString;
        }

        return (TNext)(object)this;
    }

    public TNext AsAnsiString(int size)
    {
        if (Column is not null)
        {
            Column.Type = DbType.AnsiString;
            Column.Size = size;
        }

        return (TNext)(object)this;
    }

    public TNext AsBinary()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Binary;
        }

        return (TNext)(object)this;
    }

    public TNext AsBinary(int size)
    {
        if (Column is not null)
        {
            Column.Type = DbType.Binary;
            Column.Size = size;
        }

        return (TNext)(object)this;
    }

    public TNext AsBoolean()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Boolean;
        }

        return (TNext)(object)this;
    }

    public TNext AsByte()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Byte;
        }

        return (TNext)(object)this;
    }

    public TNext AsCurrency()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Currency;
        }

        return (TNext)(object)this;
    }

    public TNext AsDate()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Date;
        }

        return (TNext)(object)this;
    }

    public TNext AsDateTime()
    {
        if (Column is not null)
        {
            Column.Type = DbType.DateTime;
        }

        return (TNext)(object)this;
    }

    public TNext AsDecimal()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Decimal;
        }

        return (TNext)(object)this;
    }

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

    public TNext AsDouble()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Double;
        }

        return (TNext)(object)this;
    }

    public TNext AsFixedLengthString(int size)
    {
        if (Column is not null)
        {
            Column.Type = DbType.StringFixedLength;
            Column.Size = size;
        }

        return (TNext)(object)this;
    }

    public TNext AsFixedLengthAnsiString(int size)
    {
        if (Column is not null)
        {
            Column.Type = DbType.AnsiStringFixedLength;
            Column.Size = size;
        }

        return (TNext)(object)this;
    }

    public TNext AsFloat()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Single;
        }

        return (TNext)(object)this;
    }

    public TNext AsGuid()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Guid;
        }

        return (TNext)(object)this;
    }

    public TNext AsInt16()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Int16;
        }

        return (TNext)(object)this;
    }

    public TNext AsInt32()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Int32;
        }

        return (TNext)(object)this;
    }

    public TNext AsInt64()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Int64;
        }

        return (TNext)(object)this;
    }

    public TNext AsString()
    {
        if (Column is not null)
        {
            Column.Type = DbType.String;
        }

        return (TNext)(object)this;
    }

    public TNext AsString(int size)
    {
        if (Column is not null)
        {
            Column.Type = DbType.String;
            Column.Size = size;
        }

        return (TNext)(object)this;
    }

    public TNext AsTime()
    {
        if (Column is not null)
        {
            Column.Type = DbType.Time;
        }

        return (TNext)(object)this;
    }

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
