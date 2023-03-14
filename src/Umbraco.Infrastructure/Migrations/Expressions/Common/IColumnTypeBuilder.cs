namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

/// <summary>
///     Builds a column type expression.
/// </summary>
public interface IColumnTypeBuilder<out TNext> : IFluentBuilder
    where TNext : IFluentBuilder
{
    TNext AsAnsiString();

    TNext AsAnsiString(int size);

    TNext AsBinary();

    TNext AsBinary(int size);

    TNext AsBoolean();

    TNext AsByte();

    TNext AsCurrency();

    TNext AsDate();

    TNext AsDateTime();

    TNext AsDecimal();

    TNext AsDecimal(int size, int precision);

    TNext AsDouble();

    TNext AsGuid();

    TNext AsFixedLengthString(int size);

    TNext AsFixedLengthAnsiString(int size);

    TNext AsFloat();

    TNext AsInt16();

    TNext AsInt32();

    TNext AsInt64();

    TNext AsString();

    TNext AsString(int size);

    TNext AsTime();

    TNext AsXml();

    TNext AsXml(int size);

    TNext AsCustom(string customType);
}
