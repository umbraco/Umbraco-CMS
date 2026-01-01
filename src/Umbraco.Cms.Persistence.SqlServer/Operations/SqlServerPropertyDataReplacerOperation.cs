using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.SqlServer.Operations;

/// <summary>
/// SQL Server implementation of <see cref="IPropertyDataReplacerOperation"/> that uses a Table-Valued Parameter
/// to call a stored procedure in a single database round trip.
/// </summary>
public class SqlServerPropertyDataReplacerOperation : IPropertyDataReplacerOperation
{
    /// <inheritdoc/>
    public string? ProviderName => Constants.ProviderName;

    /// <inheritdoc/>
    public void ReplacePropertyData(IUmbracoDatabase database, int versionId, IEnumerable<PropertyDataDto> propertyDataDtos)
    {
        // Create a DataTable that matches the umbracoPropertyDataTableType TVP structure.
        using var dataTable = new DataTable();
        dataTable.Columns.Add("versionId", typeof(int));
        dataTable.Columns.Add("propertyTypeId", typeof(int));
        dataTable.Columns.Add("languageId", typeof(int));
        dataTable.Columns.Add("segment", typeof(string));
        dataTable.Columns.Add("intValue", typeof(int));
        dataTable.Columns.Add("decimalValue", typeof(decimal));
        dataTable.Columns.Add("dateValue", typeof(DateTime));
        dataTable.Columns.Add("varcharValue", typeof(string));
        dataTable.Columns.Add("textValue", typeof(string));

        // Populate the DataTable with the property data DTOs.
        foreach (PropertyDataDto dto in propertyDataDtos)
        {
            dataTable.Rows.Add(
                dto.VersionId,
                dto.PropertyTypeId,
                dto.LanguageId.HasValue ? dto.LanguageId.Value : DBNull.Value,
                dto.Segment ?? (object)DBNull.Value,
                dto.IntegerValue.HasValue ? dto.IntegerValue.Value : DBNull.Value,
                dto.DecimalValue.HasValue ? dto.DecimalValue.Value : DBNull.Value,
                dto.DateValue.HasValue ? dto.DateValue.Value : DBNull.Value,
                dto.VarcharValue ?? (object)DBNull.Value,
                dto.TextValue ?? (object)DBNull.Value);
        }

        // Get the underlying SqlConnection from the database.
        SqlConnection connection = NPocoDatabaseExtensions.GetTypedConnection<SqlConnection>(database.Connection);

        // Get the transaction if one exists.
        SqlTransaction? transaction = null;
        using (DbCommand command = database.CreateCommand(database.Connection, CommandType.Text, string.Empty))
        {
            if (command.Transaction != null)
            {
                transaction = NPocoDatabaseExtensions.GetTypedTransaction<SqlTransaction>(command.Transaction);
            }
        }

        // Create and execute the stored procedure command.
        using var spCommand = new SqlCommand("umbracoReplacePropertyData", connection, transaction);
        spCommand.CommandType = CommandType.StoredProcedure;

        // Add the TVP parameter.
        SqlParameter tvpParam = spCommand.Parameters.AddWithValue("@propertyData", dataTable);
        tvpParam.SqlDbType = SqlDbType.Structured;
        tvpParam.TypeName = "umbracoPropertyDataTableType";

        spCommand.ExecuteNonQuery();
    }
}
