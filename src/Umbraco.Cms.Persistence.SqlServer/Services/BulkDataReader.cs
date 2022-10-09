using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Umbraco.Cms.Persistence.SqlServer.Services;

/// <summary>
///     A base implementation of <see cref="IDataReader" /> that is suitable for
///     <see cref="SqlBulkCopy.WriteToServer(IDataReader)" />.
/// </summary>
/// <remarks>
///     Borrowed from Microsoft:
///     See: https://blogs.msdn.microsoft.com/anthonybloesch/2013/01/23/bulk-loading-data-with-idatareader-and-sqlbulkcopy/
///     This implementation is designed to be very memory efficient requiring few memory resources and to support
///     rapid transfer of data to SQL Server.
///     Subclasses should implement <see cref="BulkDataReader.SchemaName" />, <see cref="BulkDataReader.TableName" />,
///     <see cref="BulkDataReader.AddSchemaTableRows()" />, <see cref="BulkDataReader.Read()" />,
///     <see cref="BulkDataReader.GetValue(int)" />.
///     If they contain disposable resources they should override <see cref="BulkDataReader.Dispose(bool)" />.
///     SD: Alternatively, we could have used a LinqEntityDataReader which is nicer to use but it uses quite a lot of
///     reflection and
///     I thought this would just be quicker.
///     Simple example of that:
///     https://github.com/gridsum/DataflowEx/blob/master/Gridsum.DataflowEx/Databases/BulkDataReader.cs
///     Full example of that:
///     https://github.com/matthewschrager/Repository/blob/master/Repository.EntityFramework/EntityDataReader.cs
///     So we know where to find that if we ever need it, these would convert any Linq data source to an IDataReader
/// </remarks>
internal abstract class BulkDataReader : IDataReader
{
    #region Fields

    /// <summary>
    ///     The <see cref="DataTable" /> containing the input row set's schema information
    ///     <see cref="SqlBulkCopy.WriteToServer(IDataReader)" />
    ///     requires to function correctly.
    /// </summary>
    private DataTable? _schemaTable = new();

    /// <summary>
    ///     The mapping from the row set input to the target table's columns.
    /// </summary>
    private List<SqlBulkCopyColumnMapping>? _columnMappings = new();

    #endregion

    #region Subclass utility routines

    /// <summary>
    ///     The mapping from the row set input to the target table's columns.
    /// </summary>
    /// <remarks>
    ///     If necessary, <see cref="BulkDataReader.AddSchemaTableRows()" /> will be called to initialize the mapping.
    /// </remarks>
    public ReadOnlyCollection<SqlBulkCopyColumnMapping> ColumnMappings
    {
        get
        {
            if (_columnMappings?.Count == 0)
            {
                // Need to add the column definitions and mappings.
                AddSchemaTableRows();

                if (_columnMappings.Count == 0)
                {
                    throw new InvalidOperationException("AddSchemaTableRows did not add rows.");
                }

                Debug.Assert(_schemaTable?.Rows.Count == FieldCount);
            }

            return new ReadOnlyCollection<SqlBulkCopyColumnMapping>(_columnMappings!);
        }
    }

    /// <summary>
    ///     The name of the input row set's schema.
    /// </summary>
    /// <remarks>
    ///     This may be different from the target schema but usually they are identical.
    /// </remarks>
    protected abstract string SchemaName
    {
        get;
    }

    /// <summary>
    ///     The name of the input row set's table.
    /// </summary>
    /// <remarks>
    ///     This may be different from the target table but usually they are identical.
    /// </remarks>
    protected abstract string TableName
    {
        get;
    }

    /// <summary>
    ///     Adds the input row set's schema to the object.
    /// </summary>
    /// <remarks>
    ///     Call
    ///     <see
    ///         cref="AddSchemaTableRow(string,Nullable{Int32},Nullable{Int16},Nullable{Int16},bool,bool,bool,SqlDbType,string,string,string,string,string)" />
    ///     to do this for each row.
    /// </remarks>
    /// <seealso
    ///     cref="AddSchemaTableRow(string,Nullable{Int32},Nullable{Int16},Nullable{Int16},bool,bool,bool,SqlDbType,string,string,string,string,string)" />
    protected abstract void AddSchemaTableRows();

    /// <summary>
    ///     For each <see cref="SqlDbType" />, the optional columns that may have values.
    /// </summary>
    /// <remarks>
    ///     This is used for checking the parameters of
    ///     <see
    ///         cref="AddSchemaTableRow(string,Nullable{Int32},Nullable{Int16},Nullable{Int16},bool,bool,bool,SqlDbType,string,string,string,string,string)" />
    ///     .
    /// </remarks>
    /// <seealso
    ///     cref="AddSchemaTableRow(string,Nullable{Int32},Nullable{Int16},Nullable{Int16},bool,bool,bool,SqlDbType,string,string,string,string,string)" />
    private static readonly Dictionary<SqlDbType, List<string>> AllowedOptionalColumnCombinations = new()
    {
        {SqlDbType.BigInt, new List<string>()},
        {SqlDbType.Binary, new List<string> {SchemaTableColumn.ColumnSize}},
        {SqlDbType.Bit, new List<string>()},
        {SqlDbType.Char, new List<string> {SchemaTableColumn.ColumnSize}},
        {SqlDbType.Date, new List<string>()},
        {SqlDbType.DateTime, new List<string>()},
        {SqlDbType.DateTime2, new List<string> {SchemaTableColumn.NumericPrecision}},
        {SqlDbType.DateTimeOffset, new List<string> {SchemaTableColumn.NumericPrecision}},
        {SqlDbType.Decimal, new List<string> {SchemaTableColumn.NumericPrecision, SchemaTableColumn.NumericScale}},
        {SqlDbType.Float, new List<string> {SchemaTableColumn.NumericPrecision, SchemaTableColumn.NumericScale}},
        {SqlDbType.Image, new List<string>()},
        {SqlDbType.Int, new List<string>()},
        {SqlDbType.Money, new List<string>()},
        {SqlDbType.NChar, new List<string> {SchemaTableColumn.ColumnSize}},
        {SqlDbType.NText, new List<string>()},
        {SqlDbType.NVarChar, new List<string> {SchemaTableColumn.ColumnSize}},
        {SqlDbType.Real, new List<string>()},
        {SqlDbType.SmallDateTime, new List<string>()},
        {SqlDbType.SmallInt, new List<string>()},
        {SqlDbType.SmallMoney, new List<string>()},
        {SqlDbType.Structured, new List<string>()},
        {SqlDbType.Text, new List<string>()},
        {SqlDbType.Time, new List<string> {SchemaTableColumn.NumericPrecision}},
        {SqlDbType.Timestamp, new List<string>()},
        {SqlDbType.TinyInt, new List<string>()},
        {SqlDbType.Udt, new List<string> {DataTypeNameSchemaColumn}},
        {SqlDbType.UniqueIdentifier, new List<string>()},
        {SqlDbType.VarBinary, new List<string> {SchemaTableColumn.ColumnSize}},
        {SqlDbType.VarChar, new List<string> {SchemaTableColumn.ColumnSize}},
        {SqlDbType.Variant, new List<string>()},
        {
            SqlDbType.Xml,
            new List<string>
            {
                XmlSchemaCollectionDatabaseSchemaColumn,
                XmlSchemaCollectionOwningSchemaSchemaColumn,
                XmlSchemaCollectionNameSchemaColumn
            }
        }
    };

    /// <summary>
    ///     A helper method to support <see cref="AddSchemaTableRows" />.
    /// </summary>
    /// <remarks>
    ///     This methods does extensive argument checks. These errors will cause hard to diagnose exceptions in latter
    ///     processing so it is important to detect them when they can be easily associated with the code defect.
    /// </remarks>
    /// <exception cref="ArgumentException">
    ///     The combination of values for the parameters is not supported.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     A null value for the parameter is not supported.
    /// </exception>
    /// <param name="columnName">
    ///     The name of the column.
    /// </param>
    /// <param name="columnSize">
    ///     The size of the column which may be null if not applicable.
    /// </param>
    /// <param name="numericPrecision">
    ///     The precision of the column which may be null if not applicable.
    /// </param>
    /// <param name="numericScale">
    ///     The scale of the column which may be null if not applicable.
    /// </param>
    /// <param name="isUnique">
    ///     Are the column values unique (i.e. never duplicated)?
    /// </param>
    /// <param name="isKey">
    ///     Is the column part of the primary key?
    /// </param>
    /// <param name="allowDbNull">
    ///     Is the column nullable (i.e. optional)?
    /// </param>
    /// <param name="providerType">
    ///     The corresponding <see cref="SqlDbType" />.
    /// </param>
    /// <param name="udtSchema">
    ///     The schema name of the UDT.
    /// </param>
    /// <param name="udtType">
    ///     The type name of the UDT.
    /// </param>
    /// <param name="xmlSchemaCollectionDatabase">
    ///     For XML columns the schema collection's database name. Otherwise, null.
    /// </param>
    /// <param name="xmlSchemaCollectionOwningSchema">
    ///     For XML columns the schema collection's schema name. Otherwise, null.
    /// </param>
    /// <param name="xmlSchemaCollectionName">
    ///     For XML columns the schema collection's name. Otherwise, null.
    /// </param>
    /// <seealso cref="AddSchemaTableRows" />
    protected void AddSchemaTableRow(
        string columnName,
        int? columnSize,
        short? numericPrecision,
        short? numericScale,
        bool isUnique,
        bool isKey,
        bool allowDbNull,
        SqlDbType providerType,
        string? udtSchema,
        string? udtType,
        string? xmlSchemaCollectionDatabase,
        string? xmlSchemaCollectionOwningSchema,
        string? xmlSchemaCollectionName)
    {
        if (string.IsNullOrEmpty(columnName))
        {
            throw new ArgumentException("columnName must be a nonempty string.");
        }

        if (columnSize.HasValue && columnSize.Value <= 0)
        {
            throw new ArgumentOutOfRangeException("columnSize");
        }

        if (numericPrecision.HasValue && numericPrecision.Value <= 0)
        {
            throw new ArgumentOutOfRangeException("numericPrecision");
        }

        if (numericScale.HasValue && numericScale.Value < 0)
        {
            throw new ArgumentOutOfRangeException("columnSize");
        }


        if (AllowedOptionalColumnCombinations.TryGetValue(providerType, out List<string>? allowedOptionalColumnList))
        {
            if ((columnSize.HasValue && !allowedOptionalColumnList.Contains(SchemaTableColumn.ColumnSize)) ||
                (numericPrecision.HasValue &&
                 !allowedOptionalColumnList.Contains(SchemaTableColumn.NumericPrecision)) ||
                (numericScale.HasValue && !allowedOptionalColumnList.Contains(SchemaTableColumn.NumericScale)) ||
                (udtSchema != null && !allowedOptionalColumnList.Contains(DataTypeNameSchemaColumn)) ||
                (udtType != null && !allowedOptionalColumnList.Contains(DataTypeNameSchemaColumn)) ||
                (xmlSchemaCollectionDatabase != null &&
                 !allowedOptionalColumnList.Contains(XmlSchemaCollectionDatabaseSchemaColumn)) ||
                (xmlSchemaCollectionOwningSchema != null &&
                 !allowedOptionalColumnList.Contains(XmlSchemaCollectionOwningSchemaSchemaColumn)) ||
                (xmlSchemaCollectionName != null &&
                 !allowedOptionalColumnList.Contains(XmlSchemaCollectionNameSchemaColumn)))
            {
                throw new ArgumentException("Columns are set that are incompatible with the value of providerType.");
            }
        }
        else
        {
            throw new ArgumentException("providerType is unsupported.");
        }

        Type dataType; // Corresponding CLR type.
        string dataTypeName; // Corresponding SQL Server type.
        var isLong = false; // Is the column a large value column (e.g. nvarchar(max))?

        switch (providerType)
        {
            case SqlDbType.BigInt:
                dataType = typeof(long);
                dataTypeName = "bigint";
                break;

            case SqlDbType.Binary:
                dataType = typeof(byte[]);

                if (!columnSize.HasValue)
                {
                    throw new ArgumentException("columnSize must be specified for \"binary\" type columns.");
                }

                if (columnSize > 8000)
                {
                    throw new ArgumentOutOfRangeException("columnSize");
                }

                dataTypeName = string.Format(
                    CultureInfo.InvariantCulture,
                    "binary({0})",
                    columnSize.Value);
                break;

            case SqlDbType.Bit:
                dataType = typeof(bool);
                dataTypeName = "bit";
                break;

            case SqlDbType.Char:
                dataType = typeof(string);

                if (!columnSize.HasValue)
                {
                    throw new ArgumentException("columnSize must be specified for \"char\" type columns.");
                }

                if (columnSize > 8000)
                {
                    throw new ArgumentOutOfRangeException("columnSize");
                }

                dataTypeName = string.Format(
                    CultureInfo.InvariantCulture,
                    "char({0})",
                    columnSize.Value);
                break;

            case SqlDbType.Date:
                dataType = typeof(DateTime);
                dataTypeName = "date";
                break;

            case SqlDbType.DateTime:
                dataType = typeof(DateTime);
                dataTypeName = "datetime";
                break;

            case SqlDbType.DateTime2:
                dataType = typeof(DateTime);

                if (numericPrecision.HasValue)
                {
                    if (numericPrecision.Value > 7)
                    {
                        throw new ArgumentOutOfRangeException("numericPrecision");
                    }

                    dataTypeName = string.Format(
                        CultureInfo.InvariantCulture,
                        "datetime2({0})",
                        numericPrecision.Value);
                }
                else
                {
                    dataTypeName = "datetime2";
                }

                break;

            case SqlDbType.DateTimeOffset:
                dataType = typeof(DateTimeOffset);

                if (numericPrecision.HasValue)
                {
                    if (numericPrecision.Value > 7)
                    {
                        throw new ArgumentOutOfRangeException("numericPrecision");
                    }

                    dataTypeName = string.Format(
                        CultureInfo.InvariantCulture,
                        "datetimeoffset({0})",
                        numericPrecision.Value);
                }
                else
                {
                    dataTypeName = "datetimeoffset";
                }

                break;

            case SqlDbType.Decimal:
                dataType = typeof(decimal);

                if (!numericPrecision.HasValue || !numericScale.HasValue)
                {
                    throw new ArgumentException(
                        "numericPrecision and numericScale must be specified for \"decimal\" type columns.");
                }

                if (numericPrecision > 38)
                {
                    throw new ArgumentOutOfRangeException("numericPrecision");
                }

                if (numericScale.Value > numericPrecision.Value)
                {
                    throw new ArgumentException(
                        "numericScale must not be larger than numericPrecision for \"decimal\" type columns.");
                }

                dataTypeName = string.Format(
                    CultureInfo.InvariantCulture,
                    "decimal({0}, {1})",
                    numericPrecision.Value,
                    numericScale.Value);
                break;

            case SqlDbType.Float:
                dataType = typeof(double);

                if (!numericPrecision.HasValue)
                {
                    throw new ArgumentException("numericPrecision must be specified for \"float\" type columns");
                }

                if (numericPrecision > 53)
                {
                    throw new ArgumentOutOfRangeException("numericPrecision");
                }

                dataTypeName = string.Format(
                    CultureInfo.InvariantCulture,
                    "float({0})",
                    numericPrecision.Value);
                break;

            case SqlDbType.Image:
                dataType = typeof(byte[]);
                dataTypeName = "image";
                break;

            case SqlDbType.Int:
                dataType = typeof(int);
                dataTypeName = "int";
                break;

            case SqlDbType.Money:
                dataType = typeof(decimal);
                dataTypeName = "money";
                break;

            case SqlDbType.NChar:
                dataType = typeof(string);

                if (!columnSize.HasValue)
                {
                    throw new ArgumentException("columnSize must be specified for \"nchar\" type columns");
                }

                if (columnSize > 4000)
                {
                    throw new ArgumentOutOfRangeException("columnSize");
                }

                dataTypeName = string.Format(
                    CultureInfo.InvariantCulture,
                    "nchar({0})",
                    columnSize.Value);
                break;

            case SqlDbType.NText:
                dataType = typeof(string);
                dataTypeName = "ntext";
                break;

            case SqlDbType.NVarChar:
                dataType = typeof(string);

                if (columnSize.HasValue)
                {
                    if (columnSize > 4000)
                    {
                        throw new ArgumentOutOfRangeException("columnSize");
                    }

                    dataTypeName = string.Format(
                        CultureInfo.InvariantCulture,
                        "nvarchar({0})",
                        columnSize.Value);
                }
                else
                {
                    isLong = true;

                    dataTypeName = "nvarchar(max)";
                }

                break;

            case SqlDbType.Real:
                dataType = typeof(float);
                dataTypeName = "real";
                break;

            case SqlDbType.SmallDateTime:
                dataType = typeof(DateTime);
                dataTypeName = "smalldatetime";
                break;

            case SqlDbType.SmallInt:
                dataType = typeof(short);
                dataTypeName = "smallint";
                break;

            case SqlDbType.SmallMoney:
                dataType = typeof(decimal);
                dataTypeName = "smallmoney";
                break;

            // SqlDbType.Structured not supported because it related to nested rowsets.

            case SqlDbType.Text:
                dataType = typeof(string);
                dataTypeName = "text";
                break;

            case SqlDbType.Time:
                dataType = typeof(TimeSpan);

                if (numericPrecision.HasValue)
                {
                    if (numericPrecision > 7)
                    {
                        throw new ArgumentOutOfRangeException("numericPrecision");
                    }

                    dataTypeName = string.Format(
                        CultureInfo.InvariantCulture,
                        "time({0})",
                        numericPrecision.Value);
                }
                else
                {
                    dataTypeName = "time";
                }

                break;


            // SqlDbType.Timestamp not supported because rowversions are not settable.

            case SqlDbType.TinyInt:
                dataType = typeof(byte);
                dataTypeName = "tinyint";
                break;

            case SqlDbType.Udt:
                if (string.IsNullOrEmpty(udtSchema))
                {
                    throw new ArgumentException("udtSchema must be nonnull and nonempty for \"UDT\" columns.");
                }

                if (string.IsNullOrEmpty(udtType))
                {
                    throw new ArgumentException("udtType must be nonnull and nonempty for \"UDT\" columns.");
                }

                dataType = typeof(object);
                using (var commandBuilder = new SqlCommandBuilder())
                {
                    dataTypeName = commandBuilder.QuoteIdentifier(udtSchema) + "." +
                                   commandBuilder.QuoteIdentifier(udtType);
                }

                break;

            case SqlDbType.UniqueIdentifier:
                dataType = typeof(Guid);
                dataTypeName = "uniqueidentifier";
                break;

            case SqlDbType.VarBinary:
                dataType = typeof(byte[]);

                if (columnSize.HasValue)
                {
                    if (columnSize > 8000)
                    {
                        throw new ArgumentOutOfRangeException("columnSize");
                    }

                    dataTypeName = string.Format(
                        CultureInfo.InvariantCulture,
                        "varbinary({0})",
                        columnSize.Value);
                }
                else
                {
                    isLong = true;

                    dataTypeName = "varbinary(max)";
                }

                break;

            case SqlDbType.VarChar:
                dataType = typeof(string);

                if (columnSize.HasValue)
                {
                    if (columnSize > 8000)
                    {
                        throw new ArgumentOutOfRangeException("columnSize");
                    }

                    dataTypeName = string.Format(
                        CultureInfo.InvariantCulture,
                        "varchar({0})",
                        columnSize.Value);
                }
                else
                {
                    isLong = true;

                    dataTypeName = "varchar(max)";
                }

                break;

            case SqlDbType.Variant:
                dataType = typeof(object);
                dataTypeName = "sql_variant";
                break;

            case SqlDbType.Xml:
                dataType = typeof(string);

                if (xmlSchemaCollectionName == null)
                {
                    if (xmlSchemaCollectionDatabase != null || xmlSchemaCollectionOwningSchema != null)
                    {
                        throw new ArgumentException(
                            "xmlSchemaCollectionDatabase and xmlSchemaCollectionOwningSchema must be null if xmlSchemaCollectionName is null for \"xml\" columns.");
                    }

                    dataTypeName = "xml";
                }
                else
                {
                    if (xmlSchemaCollectionName.Length == 0)
                    {
                        throw new ArgumentException(
                            "xmlSchemaCollectionName must be nonempty or null for \"xml\" columns.");
                    }

                    if (xmlSchemaCollectionDatabase != null &&
                        xmlSchemaCollectionDatabase.Length == 0)
                    {
                        throw new ArgumentException(
                            "xmlSchemaCollectionDatabase must be null or nonempty for \"xml\" columns.");
                    }

                    if (xmlSchemaCollectionOwningSchema != null &&
                        xmlSchemaCollectionOwningSchema.Length == 0)
                    {
                        throw new ArgumentException(
                            "xmlSchemaCollectionOwningSchema must be null or nonempty for \"xml\" columns.");
                    }

                    var schemaCollection = new StringBuilder("xml(");

                    if (xmlSchemaCollectionDatabase != null)
                    {
                        schemaCollection.Append("[" + xmlSchemaCollectionDatabase + "]");
                    }

                    schemaCollection.Append("[" + (xmlSchemaCollectionOwningSchema ?? SchemaName) + "]");
                    schemaCollection.Append("[" + xmlSchemaCollectionName + "]");

                    dataTypeName = schemaCollection.ToString();
                }

                break;

            default:
                throw new ArgumentOutOfRangeException("providerType");
        }

        _schemaTable?.Rows.Add(
            columnName,
            _schemaTable.Rows.Count,
            columnSize,
            numericPrecision,
            numericScale,
            isUnique,
            isKey,
            "TraceServer",
            "TraceWarehouse",
            columnName,
            SchemaName,
            TableName,
            dataType,
            allowDbNull,
            providerType,
            false, // isAliased
            false, // isExpression
            false, // isIdentity,
            false, // isAutoIncrement,
            false, // isRowVersion,
            false, // isHidden,
            isLong,
            true, // isReadOnly,
            dataType,
            dataTypeName,
            xmlSchemaCollectionDatabase,
            xmlSchemaCollectionOwningSchema,
            xmlSchemaCollectionName);

        _columnMappings?.Add(new SqlBulkCopyColumnMapping(columnName, columnName));
    }

    #endregion

    #region Constructors

    private const string IsIdentitySchemaColumn = "IsIdentity";

    private const string DataTypeNameSchemaColumn = "DataTypeName";

    private const string XmlSchemaCollectionDatabaseSchemaColumn = "XmlSchemaCollectionDatabase";

    private const string XmlSchemaCollectionOwningSchemaSchemaColumn = "XmlSchemaCollectionOwningSchema";

    private const string XmlSchemaCollectionNameSchemaColumn = "XmlSchemaCollectionName";

    /// <summary>
    ///     Constructor.
    /// </summary>
    protected BulkDataReader()
    {
        _schemaTable.Locale = CultureInfo.InvariantCulture;

        DataColumnCollection columns = _schemaTable.Columns;

        columns.Add(SchemaTableColumn.ColumnName, typeof(string));
        columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int));
        columns.Add(SchemaTableColumn.ColumnSize, typeof(int));
        columns.Add(SchemaTableColumn.NumericPrecision, typeof(short));
        columns.Add(SchemaTableColumn.NumericScale, typeof(short));
        columns.Add(SchemaTableColumn.IsUnique, typeof(bool));
        columns.Add(SchemaTableColumn.IsKey, typeof(bool));
        columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string));
        columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(string));
        columns.Add(SchemaTableColumn.BaseColumnName, typeof(string));
        columns.Add(SchemaTableColumn.BaseSchemaName, typeof(string));
        columns.Add(SchemaTableColumn.BaseTableName, typeof(string));
        columns.Add(SchemaTableColumn.DataType, typeof(Type));
        columns.Add(SchemaTableColumn.AllowDBNull, typeof(bool));
        columns.Add(SchemaTableColumn.ProviderType, typeof(int));
        columns.Add(SchemaTableColumn.IsAliased, typeof(bool));
        columns.Add(SchemaTableColumn.IsExpression, typeof(bool));
        columns.Add(IsIdentitySchemaColumn, typeof(bool));
        columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool));
        columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(bool));
        columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(bool));
        columns.Add(SchemaTableColumn.IsLong, typeof(bool));
        columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(bool));
        columns.Add(SchemaTableOptionalColumn.ProviderSpecificDataType, typeof(Type));
        columns.Add(DataTypeNameSchemaColumn, typeof(string));
        columns.Add(XmlSchemaCollectionDatabaseSchemaColumn, typeof(string));
        columns.Add(XmlSchemaCollectionOwningSchemaSchemaColumn, typeof(string));
        columns.Add(XmlSchemaCollectionNameSchemaColumn, typeof(string));
    }

    #endregion

    #region IDataReader

    /// <summary>
    ///     Gets a value indicating the depth of nesting for the current row. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <remarks>
    ///     <see cref="SqlBulkCopy" /> does not support nested result sets so this method always returns 0.
    /// </remarks>
    /// <seealso cref="IDataReader.Depth" />
    public int Depth => 0;

    /// <summary>
    ///     Gets the number of columns in the current row. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <seealso cref="IDataRecord.FieldCount" />
    public int FieldCount => GetSchemaTable().Rows.Count;

    /// <summary>
    ///     Is the bulk copy process open?
    /// </summary>
    private bool _isOpen = true;

    /// <summary>
    ///     Gets a value indicating whether the data reader is closed. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <seealso cref="IDataReader.IsClosed" />
    public bool IsClosed => !_isOpen;

    /// <summary>
    ///     Gets the column located at the specified index. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     No column with the specified index was found.
    /// </exception>
    /// <param name="i">
    ///     The zero-based index of the column to get.
    /// </param>
    /// <returns>
    ///     The column located at the specified index as an <see cref="object" />.
    /// </returns>
    /// <seealso cref="P:IDataRecord.Item(Int32)" />
    public object this[int i] => GetValue(i);

    /// <summary>
    ///     Gets the column with the specified name. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     No column with the specified name was found.
    /// </exception>
    /// <param name="name">
    ///     The name of the column to find.
    /// </param>
    /// <returns>
    ///     The column located at the specified name as an <see cref="object" />.
    /// </returns>
    /// <seealso cref="P:IDataRecord.Item(String)" />
    public object this[string name] => GetValue(GetOrdinal(name));

    /// <summary>
    ///     Gets the number of rows changed, inserted, or deleted by execution of the SQL statement. (Inherited from
    ///     <see cref="IDataReader" />.)
    /// </summary>
    /// <remarks>
    ///     Always returns -1 which is the expected behaviour for statements.
    /// </remarks>
    /// <seealso cref="IDataReader.RecordsAffected" />
    public virtual int RecordsAffected => -1;

    /// <summary>
    ///     Closes the <see cref="IDataReader" />. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <seealso cref="IDataReader.Close" />
    public void Close() => _isOpen = false;

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="bool" />.  (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    /// <seealso cref="IDataRecord.GetBoolean(int)" />
    public bool GetBoolean(int i) => (bool)GetValue(i);

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="byte" />.  (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    /// <seealso cref="IDataRecord.GetByte(int)" />
    public byte GetByte(int i) => (byte)GetValue(i);

    /// <summary>
    ///     Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer
    ///     offset.
    ///     (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <remarks>
    ///     If you pass a buffer that is null, <see cref="GetBytes" /> returns the length of the row in bytes.
    /// </remarks>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <param name="fieldOffset">
    ///     The index within the field from which to start the read operation.
    /// </param>
    /// <param name="buffer">
    ///     The buffer into which to read the stream of bytes.
    /// </param>
    /// <param name="bufferoffset">
    ///     The index for buffer to start the read operation.
    /// </param>
    /// <param name="length">
    ///     The number of bytes to read.
    /// </param>
    /// <returns>
    ///     The actual number of bytes read.
    /// </returns>
    /// <seealso cref="IDataRecord.GetBytes(int,long,byte[],int,int)" />
    public long GetBytes(
        int i,
        long fieldOffset,
        byte[]? buffer,
        int bufferoffset,
        int length)
    {
        var data = (byte[])GetValue(i);

        if (buffer != null)
        {
            Array.Copy(data, fieldOffset, buffer, bufferoffset, length);
        }

        return data.LongLength;
    }

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="char" />.  (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    /// <seealso cref="IDataRecord.GetChar(int)" />
    public char GetChar(int i)
    {
        char result;

        var data = GetValue(i);
        var dataAsChar = data as char?;

        if (dataAsChar.HasValue)
        {
            result = dataAsChar.Value;
        }
        else if (data is char[] dataAsCharArray &&
                 dataAsCharArray.Length == 1)
        {
            result = dataAsCharArray[0];
        }
        else if (data is string dataAsString &&
                 dataAsString.Length == 1)
        {
            result = dataAsString[0];
        }
        else
        {
            throw new InvalidOperationException("GetValue did not return a Char compatible type.");
        }

        return result;
    }

    /// <summary>
    ///     Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given
    ///     buffer offset.
    ///     (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <remarks>
    ///     If you pass a buffer that is null, <see cref="GetChars" /> returns the length of the row in bytes.
    /// </remarks>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <param name="fieldoffset">
    ///     The index within the field from which to start the read operation.
    /// </param>
    /// <param name="buffer">
    ///     The buffer into which to read the stream of characters.
    /// </param>
    /// <param name="bufferoffset">
    ///     The index for buffer to start the read operation.
    /// </param>
    /// <param name="length">
    ///     The number of characters to read.
    /// </param>
    /// <returns>
    ///     The actual number of characters read.
    /// </returns>
    /// <seealso cref="IDataRecord.GetChars(int,long,char[],int,int)" />
    public long GetChars(
        int i,
        long fieldoffset,
        char[]? buffer,
        int bufferoffset,
        int length)
    {
        var data = GetValue(i);

        var dataAsCharArray = data as char[];

        if (data is string dataAsString)
        {
            dataAsCharArray = dataAsString.ToCharArray((int)fieldoffset, length);
        }
        else if (dataAsCharArray == null)
        {
            throw new InvalidOperationException("GetValue did not return either a Char array or a String.");
        }

        if (buffer != null)
        {
            Array.Copy(dataAsCharArray, fieldoffset, buffer, bufferoffset, length);
        }

        return dataAsCharArray.LongLength;
    }

    /// <summary>
    ///     Returns an IDataReader for the specified column ordinal. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <remarks>
    ///     <see cref="SqlBulkCopy" /> does not support nested result sets so this method always returns null.
    /// </remarks>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The <see cref="IDataReader" /> for the specified column ordinal (null).
    /// </returns>
    /// <seealso cref="IDataRecord.GetData(int)" />
    public IDataReader GetData(int i)
    {
        if (i < 0 || i >= FieldCount)
        {
            throw new ArgumentOutOfRangeException("i");
        }

        return null!;
    }

    /// <summary>
    ///     The data type information for the specified field. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The data type information for the specified field.
    /// </returns>
    /// <seealso cref="IDataRecord.GetDataTypeName(int)" />
    public string GetDataTypeName(int i) => GetFieldType(i).Name;

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="DateTime" />.  (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    /// <seealso cref="IDataRecord.GetDateTime(int)" />
    public DateTime GetDateTime(int i) => (DateTime)GetValue(i);

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="DateTimeOffset" />.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    public DateTimeOffset GetDateTimeOffset(int i) => (DateTimeOffset)GetValue(i);

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="decimal" />.  (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    /// <seealso cref="IDataRecord.GetDecimal(int)" />
    public decimal GetDecimal(int i) => (decimal)GetValue(i);

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="double" />.  (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    /// <seealso cref="IDataRecord.GetDouble(int)" />
    public double GetDouble(int i) => (double)GetValue(i);

    /// <summary>
    ///     Gets the <see cref="Type" /> information corresponding to the type of <see cref="object" /> that would be returned
    ///     from <see cref="GetValue" />.
    ///     (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The <see cref="Type" /> information corresponding to the type of <see cref="object" /> that would be returned from
    ///     <see cref="GetValue" />.
    /// </returns>
    /// <seealso cref="IDataRecord.GetFieldType(int)" />
    public Type GetFieldType(int i) => (Type)GetSchemaTable().Rows[i][SchemaTableColumn.DataType];

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="float" />.  (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    /// <seealso cref="IDataRecord.GetFloat(int)" />
    public float GetFloat(int i) => (float)this[i];

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="Guid" />.  (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    /// <seealso cref="IDataRecord.GetGuid(int)" />
    public Guid GetGuid(int i) => (Guid)GetValue(i);

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="short" />.  (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    /// <seealso cref="IDataRecord.GetInt16(int)" />
    public short GetInt16(int i) => (short)GetValue(i);

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="int" />.  (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    /// <seealso cref="IDataRecord.GetInt32(int)" />
    public int GetInt32(int i) => (int)GetValue(i);

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="long" />.  (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    /// <seealso cref="IDataRecord.GetInt64(int)" />
    public long GetInt64(int i) => (long)GetValue(i);

    /// <summary>
    ///     Gets the name for the field to find. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The name of the field or the empty string (""), if there is no value to return.
    /// </returns>
    /// <seealso cref="IDataRecord.GetName(int)" />
    public string GetName(int i) => (string)GetSchemaTable().Rows[i][SchemaTableColumn.ColumnName];

    /// <summary>
    ///     Return the index of the named field. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index of the named field was not found.
    /// </exception>
    /// <param name="name">
    ///     The name of the field to find.
    /// </param>
    /// <returns>
    ///     The index of the named field.
    /// </returns>
    /// <seealso cref="IDataRecord.GetOrdinal(string)" />
    public int GetOrdinal(string name)
    {
        if (name == null) // Empty strings are handled as a IndexOutOfRangeException.
        {
            throw new ArgumentNullException("name");
        }

        var result = -1;

        var rowCount = FieldCount;

        DataRowCollection schemaRows = GetSchemaTable().Rows;

        // Case sensitive search
        for (var ordinal = 0; ordinal < rowCount; ordinal++)
        {
            if (string.Equals((string)schemaRows[ordinal][SchemaTableColumn.ColumnName], name, StringComparison.Ordinal))
            {
                result = ordinal;
            }
        }

        if (result == -1)
        {
            // Case insensitive search.
            for (var ordinal = 0; ordinal < rowCount; ordinal++)
            {
                if (string.Equals((string)schemaRows[ordinal][SchemaTableColumn.ColumnName], name, StringComparison.OrdinalIgnoreCase))
                {
                    result = ordinal;
                }
            }
        }

        if (result == -1)
        {
            throw new IndexOutOfRangeException(name);
        }

        return result;
    }

    /// <summary>
    ///     Returns a <see cref="DataTable" /> that describes the column metadata of the <see cref="IDataReader" />. (Inherited
    ///     from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     The <see cref="IDataReader" /> is closed.
    /// </exception>
    /// <returns>
    ///     A <see cref="DataTable" /> that describes the column metadata.
    /// </returns>
    /// <seealso cref="IDataReader.GetSchemaTable()" />
    public DataTable GetSchemaTable()
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("The IDataReader is closed.");
        }

        if (_schemaTable?.Rows.Count == 0)
        {
            // Need to add the column definitions and mappings
            _schemaTable.TableName = TableName;

            AddSchemaTableRows();

            Debug.Assert(_schemaTable.Rows.Count == FieldCount);
        }

        return _schemaTable!;
    }

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="string" />. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    /// <seealso cref="IDataRecord.GetString(int)" />
    public string GetString(int i) => (string)GetValue(i);

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="TimeSpan" />.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    public TimeSpan GetTimeSpan(int i) => (TimeSpan)GetValue(i);

    /// <summary>
    ///     Gets the value of the specified column as a <see cref="object" />. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     The value of the column.
    /// </returns>
    /// <seealso cref="IDataRecord.GetValue(int)" />
    public abstract object GetValue(int i);

    /// <summary>
    ///     Populates an array of objects with the column values of the current record. (Inherited from
    ///     <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="values" /> was null.
    /// </exception>
    /// <param name="values">
    ///     An array of <see cref="object" /> to copy the attribute fields into.
    /// </param>
    /// <returns>
    ///     The number of instances of <see cref="object" /> in the array.
    /// </returns>
    /// <seealso cref="IDataRecord.GetValues(object[])" />
    public int GetValues(object[] values)
    {
        if (values == null)
        {
            throw new ArgumentNullException("values");
        }

        var fieldCount = Math.Min(FieldCount, values.Length);

        for (var i = 0; i < fieldCount; i++)
        {
            values[i] = GetValue(i);
        }

        return fieldCount;
    }

    /// <summary>
    ///     Return whether the specified field is set to null. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">
    ///     The index passed was outside the range of 0 through <see cref="FieldCount" />.
    /// </exception>
    /// <param name="i">
    ///     The zero-based column ordinal.
    /// </param>
    /// <returns>
    ///     True if the specified field is set to null; otherwise, false.
    /// </returns>
    /// <seealso cref="IDataRecord.IsDBNull(int)" />
    public bool IsDBNull(int i)
    {
        var data = GetValue(i);

        return data == null || Convert.IsDBNull(data);
    }

    /// <summary>
    ///     Advances the data reader to the next result, when reading the results of batch SQL statements. (Inherited from
    ///     <see cref="IDataReader" />.)
    /// </summary>
    /// <remarks>
    ///     <see cref="IDataReader" /> for <see cref="SqlBulkCopy" /> returns a single result set so false is always returned.
    /// </remarks>
    /// <returns>
    ///     True if there are more rows; otherwise, false. <see cref="IDataReader" /> for <see cref="SqlBulkCopy" /> returns a
    ///     single result set so false is always returned.
    /// </returns>
    /// <seealso cref="IDataReader.NextResult()" />
    public bool NextResult() => false;

    /// <summary>
    ///     Advances the <see cref="IDataReader" /> to the next record. (Inherited from <see cref="IDataReader" />.)
    /// </summary>
    /// <returns>
    ///     True if there are more rows; otherwise, false.
    /// </returns>
    /// <seealso cref="IDataReader.Read()" />
    public abstract bool Read();

    #endregion

    #region IDisposable

    /// <summary>
    ///     Has the object been disposed?
    /// </summary>
    private bool _disposed;

    /// <summary>
    ///     Dispose of any disposable and expensive resources.
    /// </summary>
    /// <param name="disposing">
    ///     Is this call the result of a <see cref="IDisposable.Dispose" /> call?
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;

            if (disposing)
            {
                if (_schemaTable != null)
                {
                    _schemaTable.Dispose();
                    _schemaTable = null;
                }

                _columnMappings = null;

                _isOpen = false;

                GC.SuppressFinalize(this);
            }
        }
    }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. (Inherited
    ///     from <see cref="IDataReader" />.)
    /// </summary>
    /// <seealso cref="IDisposable.Dispose()" />
    public void Dispose() => Dispose(true);

    /// <summary>
    ///     Finalizer
    /// </summary>
    /// <remarks>
    ///     <see cref="BulkDataReader" /> has no unmanaged resources but a subclass may thus a finalizer is required.
    /// </remarks>
    ~BulkDataReader() => Dispose(false);

    #endregion
}
