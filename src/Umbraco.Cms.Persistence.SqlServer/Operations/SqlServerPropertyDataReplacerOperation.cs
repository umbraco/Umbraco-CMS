using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.SqlServer.Operations;

/// <summary>
/// SQL Server implementation of <see cref="IPropertyDataReplacerOperation"/> that uses SqlBulkCopy
/// with a temp table and MERGE statement for optimized performance.
/// </summary>
/// <remarks>
/// This approach combines:
/// - SqlBulkCopy for fast data transfer to the server.
/// - Temp table to stage the data.
/// - MERGE statement for atomic UPDATE/INSERT/DELETE.
/// </remarks>
public class SqlServerPropertyDataReplacerOperation : IPropertyDataReplacerOperation
{
    private const string TempTableName = "#umbracoPropertyDataStaging";

    private static readonly string[] _columnNames =
    [
        "versionId", "propertyTypeId", "languageId", "segment",
        "intValue", "decimalValue", "dateValue", "varcharValue", "textValue"
    ];

    private const string CreateTempTableSql = $"""
        CREATE TABLE [{TempTableName}] (
            [versionId] INT NOT NULL,
            [propertyTypeId] INT NOT NULL,
            [languageId] INT NULL,
            [segment] NVARCHAR(256) NULL,
            [intValue] INT NULL,
            [decimalValue] DECIMAL(38, 6) NULL,
            [dateValue] DATETIME NULL,
            [varcharValue] NVARCHAR(512) NULL,
            [textValue] NVARCHAR(MAX) NULL
        );
        """;

    private const string MergeAndCleanupSql = $"""
        -- Get distinct versionIds from the staged data.
        DECLARE @versionIds TABLE (versionId INT PRIMARY KEY);
        INSERT INTO @versionIds (versionId)
        SELECT DISTINCT versionId FROM [{TempTableName}];

        -- Lock existing rows for the affected versionIds.
        SELECT id FROM [umbracoPropertyData] WITH (UPDLOCK, HOLDLOCK)
        WHERE versionId IN (SELECT versionId FROM @versionIds);

        -- MERGE: UPDATE existing, INSERT new, DELETE removed.
        MERGE [umbracoPropertyData] AS target
        USING [{TempTableName}] AS source
        ON (
            target.versionId = source.versionId
            AND target.propertyTypeId = source.propertyTypeId
            AND (target.languageId = source.languageId OR (target.languageId IS NULL AND source.languageId IS NULL))
            AND (target.segment = source.segment OR (target.segment IS NULL AND source.segment IS NULL))
        )
        WHEN MATCHED THEN
            UPDATE SET
                intValue = source.intValue,
                decimalValue = source.decimalValue,
                dateValue = source.dateValue,
                varcharValue = source.varcharValue,
                textValue = source.textValue
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (versionId, propertyTypeId, languageId, segment, intValue, decimalValue, dateValue, varcharValue, textValue)
            VALUES (source.versionId, source.propertyTypeId, source.languageId, source.segment,
                    source.intValue, source.decimalValue, source.dateValue, source.varcharValue, source.textValue)
        WHEN NOT MATCHED BY SOURCE AND target.versionId IN (SELECT versionId FROM @versionIds) THEN
            DELETE;

        -- Clean up the temp table.
        DROP TABLE [{TempTableName}];
        """;

    /// <inheritdoc/>
    public string? ProviderName => Constants.ProviderName;

    /// <inheritdoc/>
    public void ReplacePropertyData(IUmbracoDatabase database, int versionId, IEnumerable<PropertyDataDto> propertyDataDtos)
    {
        // Get the underlying SqlConnection and transaction.
        SqlConnection connection = NPocoDatabaseExtensions.GetTypedConnection<SqlConnection>(database.Connection);
        SqlTransaction? transaction = GetTransaction(database);

        // Step 1: Create the temp table.
        using (var createCmd = new SqlCommand(CreateTempTableSql, connection, transaction))
        {
            createCmd.ExecuteNonQuery();
        }

        // Step 2: Bulk copy the data into the temp table using SqlBulkCopy.
        using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
        {
            bulkCopy.DestinationTableName = TempTableName;
            bulkCopy.BulkCopyTimeout = 0; // Use connection timeout
            bulkCopy.BatchSize = 4096; // Consistent with SqlServerBulkSqlInsertProvider

            // Map columns explicitly by name.
            foreach (var columnName in _columnNames)
            {
                bulkCopy.ColumnMappings.Add(columnName, columnName);
            }

            using var reader = new PropertyDataDtoDataReader(propertyDataDtos);
            bulkCopy.WriteToServer(reader);
        }

        // Step 3: Execute the MERGE statement and clean up.
        using (var mergeCmd = new SqlCommand(MergeAndCleanupSql, connection, transaction))
        {
            mergeCmd.ExecuteNonQuery();
        }
    }

    private static SqlTransaction? GetTransaction(IUmbracoDatabase database)
    {
        using DbCommand command = database.CreateCommand(database.Connection, CommandType.Text, string.Empty);
        return command.Transaction != null
            ? NPocoDatabaseExtensions.GetTypedTransaction<SqlTransaction>(command.Transaction)
            : null;
    }

    /// <summary>
    /// A lightweight IDataReader implementation for streaming PropertyDataDto to SqlBulkCopy.
    /// </summary>
    private sealed class PropertyDataDtoDataReader : IDataReader
    {
        private readonly IEnumerator<PropertyDataDto> _enumerator;
        private PropertyDataDto? _current;

        public PropertyDataDtoDataReader(IEnumerable<PropertyDataDto> dtos)
            => _enumerator = dtos.GetEnumerator();

        public int FieldCount => _columnNames.Length;

        public bool Read()
        {
            if (_enumerator.MoveNext())
            {
                _current = _enumerator.Current;
                return true;
            }

            _current = null;
            return false;
        }

        public object GetValue(int i)
        {
            if (_current == null)
            {
                throw new InvalidOperationException("No current row.");
            }

            return i switch
            {
                0 => _current.VersionId,
                1 => _current.PropertyTypeId,
                2 => _current.LanguageId.HasValue ? _current.LanguageId.Value : DBNull.Value,
                3 => _current.Segment ?? (object)DBNull.Value,
                4 => _current.IntegerValue.HasValue ? _current.IntegerValue.Value : DBNull.Value,
                5 => _current.DecimalValue.HasValue ? _current.DecimalValue.Value : DBNull.Value,
                6 => _current.DateValue.HasValue ? _current.DateValue.Value : DBNull.Value,
                7 => _current.VarcharValue ?? (object)DBNull.Value,
                8 => _current.TextValue ?? (object)DBNull.Value,
                _ => throw new IndexOutOfRangeException($"Column index {i} is out of range."),
            };
        }

        public string GetName(int i) => _columnNames[i];

        public int GetOrdinal(string name) => Array.IndexOf(_columnNames, name);

        public void Dispose() => _enumerator.Dispose();

        // Required IDataReader members (minimal implementation for SqlBulkCopy)
        public void Close() => Dispose();

        public int Depth => 0;

        public bool IsClosed => false;

        public int RecordsAffected => -1;

        public DataTable GetSchemaTable() => throw new NotImplementedException();

        public bool NextResult() => false;

        // IDataRecord members
        public bool GetBoolean(int i) => throw new NotImplementedException();

        public byte GetByte(int i) => throw new NotImplementedException();

        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();

        public char GetChar(int i) => throw new NotImplementedException();

        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();

        public IDataReader GetData(int i) => throw new NotImplementedException();

        public string GetDataTypeName(int i) => throw new NotImplementedException();

        public DateTime GetDateTime(int i) => throw new NotImplementedException();

        public decimal GetDecimal(int i) => throw new NotImplementedException();

        public double GetDouble(int i) => throw new NotImplementedException();

        public Type GetFieldType(int i) => throw new NotImplementedException();

        public float GetFloat(int i) => throw new NotImplementedException();

        public Guid GetGuid(int i) => throw new NotImplementedException();

        public short GetInt16(int i) => throw new NotImplementedException();

        public int GetInt32(int i) => throw new NotImplementedException();

        public long GetInt64(int i) => throw new NotImplementedException();

        public string GetString(int i) => throw new NotImplementedException();

        public int GetValues(object[] values) => throw new NotImplementedException();

        public bool IsDBNull(int i) => GetValue(i) == DBNull.Value;

        public object this[int i] => GetValue(i);

        public object this[string name] => GetValue(GetOrdinal(name));
    }
}
