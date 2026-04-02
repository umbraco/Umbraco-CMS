namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

/// <summary>
///     Builds a column type expression.
/// </summary>
public interface IColumnTypeBuilder<out TNext> : IFluentBuilder
    where TNext : IFluentBuilder
{
    /// <summary>
    /// Sets the column type to an ANSI (non-Unicode) string.
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsAnsiString();

    /// <summary>Defines the column type as an ANSI string with the specified size.</summary>
    /// <param name="size">The maximum size of the ANSI string.</param>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsAnsiString(int size);

    /// <summary>
    /// Sets the column type to a binary data type.
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsBinary();

    /// <summary>
    /// Sets the column type to binary with the specified size in bytes.
    /// </summary>
    /// <param name="size">The maximum number of bytes for the binary column.</param>
    /// <returns>An instance representing the next step in the column type builder chain.</returns>
    TNext AsBinary(int size);

    /// <summary>
    /// Sets the column type to boolean.
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsBoolean();

    /// <summary>
    /// Specifies that the column should have a byte data type.
    /// </summary>
    /// <returns>The next step in the column type builder fluent interface.</returns>
    TNext AsByte();

    /// <summary>
    /// Specifies that the column should use a currency-compatible data type.
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsCurrency();

    /// <summary>
    /// Sets the column type to a date type in the database schema.
    /// </summary>
    /// <returns>An object representing the next step in the migration expression.</returns>
    TNext AsDate();

    /// <summary>
    /// Sets the column type to <c>DateTime</c>.
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsDateTime();

    /// <summary>
    /// Sets the column type to a decimal data type in the database schema.
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsDecimal();

    /// <summary>
    /// Sets the column type to decimal with the specified size and precision.
    /// </summary>
    /// <param name="size">The total number of digits that the decimal column can store.</param>
    /// <param name="precision">The number of digits to the right of the decimal point.</param>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsDecimal(int size, int precision);

    /// <summary>
    /// Sets the column type to a double-precision floating-point number.
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsDouble();

    /// <summary>
    /// Sets the column type to GUID (Globally Unique Identifier).
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsGuid();

    /// <summary>Defines the column as a fixed length string with the specified size.</summary>
    /// <param name="size">The fixed length size of the string.</param>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsFixedLengthString(int size);

    /// <summary>
    /// Sets the column type to a fixed-length ANSI string of the specified size.
    /// </summary>
    /// <param name="size">The length of the fixed-length ANSI string column.</param>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsFixedLengthAnsiString(int size);

    /// <summary>
    /// Sets the database column type to a floating-point number.
    /// </summary>
    /// <returns>The next builder in the expression chain.</returns>
    TNext AsFloat();

    /// <summary>
    /// Sets the column type to <c>Int16</c> (short).
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsInt16();

    /// <summary>
    /// Sets the column type to a 32-bit integer (Int32).
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsInt32();

    /// <summary>
    /// Sets the column type to <c>Int64</c> (a 64-bit integer).
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsInt64();

    /// <summary>
    /// Sets the column type to a string (typically maps to a VARCHAR or NVARCHAR in the database).
    /// </summary>
    /// <returns>An instance of <typeparamref name="TNext"/> to continue building the column definition.</returns>
    TNext AsString();

    /// <summary>
    /// Specifies that the column should be of type string with a maximum length.
    /// </summary>
    /// <param name="size">The maximum number of characters allowed in the string column.</param>
    /// <returns>An object representing the next step in the column type builder chain.</returns>
    TNext AsString(int size);

    /// <summary>
    /// Specifies that the column type should be set to a time data type.
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsTime();

    /// <summary>
    /// Specifies that the column type should be XML.
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsXml();

    /// <summary>
    /// Defines the column type as XML.
    /// </summary>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsXml(int size);

    /// <summary>
    /// Specifies a custom database column type for the column being defined.
    /// </summary>
    /// <param name="customType">A string representing the custom database column type.</param>
    /// <returns>The next step in the column type builder chain.</returns>
    TNext AsCustom(string customType);
}
