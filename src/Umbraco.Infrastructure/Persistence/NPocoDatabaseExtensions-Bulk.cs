using System.Data;
using System.Data.Common;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to NPoco Database class.
/// </summary>
public static partial class NPocoDatabaseExtensions
{
    /// <summary>
    ///     Determines whether a column should be part of a bulk-insert.
    /// </summary>
    /// <param name="pocoData">The PocoData object corresponding to the record's type.</param>
    /// <param name="column">The column.</param>
    /// <returns>A value indicating whether the column should be part of the bulk-insert.</returns>
    /// <remarks>Columns that are primary keys and auto-incremental, or result columns, are excluded from bulk-inserts.</remarks>
    public static bool IncludeColumn(PocoData pocoData, KeyValuePair<string, PocoColumn> column) =>
        column.Value.ResultColumn == false
        && (pocoData.TableInfo.AutoIncrement == false || column.Key != pocoData.TableInfo.PrimaryKey);
}
