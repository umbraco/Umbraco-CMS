// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;

namespace Umbraco.Extensions;

/// <summary>
///     Static and extension methods for the DataTable object
/// </summary>
public static class DataTableExtensions
{
    /// <summary>
    ///     Creates a DataTable with the specified alias and columns and uses a callback to populate the headers.
    /// </summary>
    /// <param name="tableAlias"></param>
    /// <param name="getHeaders"></param>
    /// <param name="rowData"> </param>
    /// <returns></returns>
    /// <remarks>
    ///     This has been migrated from the Node class and uses proper locking now. It is now used by the Node class and the
    ///     DynamicPublishedContent extensions for legacy reasons.
    /// </remarks>
    public static DataTable GenerateDataTable(
        string tableAlias,
        Func<string, IEnumerable<KeyValuePair<string, string>>> getHeaders,
        Func<IEnumerable<Tuple<IEnumerable<KeyValuePair<string, object?>>, IEnumerable<KeyValuePair<string, object?>>>>>
            rowData)
    {
        var dt = new DataTable(tableAlias);

        // get all row data
        Tuple<IEnumerable<KeyValuePair<string, object?>>, IEnumerable<KeyValuePair<string, object?>>>[] tableData =
            rowData().ToArray();

        // get all headers
        IDictionary<string, string> propertyHeaders = GetPropertyHeaders(tableAlias, getHeaders);
        foreach (KeyValuePair<string, string> h in propertyHeaders)
        {
            dt.Columns.Add(new DataColumn(h.Value));
        }

        // add row data
        foreach (Tuple<IEnumerable<KeyValuePair<string, object?>>, IEnumerable<KeyValuePair<string, object?>>> r in
                 tableData)
        {
            dt.PopulateRow(
                propertyHeaders,
                r.Item1,
                r.Item2);
        }

        return dt;
    }

    /// <summary>
    ///     Helper method to return this ugly object
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     This is for legacy code, I didn't want to go creating custom classes for these
    /// </remarks>
    public static List<Tuple<IEnumerable<KeyValuePair<string, object?>>, IEnumerable<KeyValuePair<string, object?>>>>
        CreateTableData() =>
        new List<Tuple<IEnumerable<KeyValuePair<string, object?>>, IEnumerable<KeyValuePair<string, object?>>>>();

    /// <summary>
    ///     Helper method to deal with these ugly objects
    /// </summary>
    /// <param name="rowData"></param>
    /// <param name="standardVals"></param>
    /// <param name="userVals"></param>
    /// <remarks>
    ///     This is for legacy code, I didn't want to go creating custom classes for these
    /// </remarks>
    public static void AddRowData(
        List<Tuple<IEnumerable<KeyValuePair<string, object?>>, IEnumerable<KeyValuePair<string, object?>>>> rowData,
        IEnumerable<KeyValuePair<string, object?>> standardVals,
        IEnumerable<KeyValuePair<string, object?>> userVals) =>
        rowData.Add(new Tuple<IEnumerable<KeyValuePair<string, object?>>, IEnumerable<KeyValuePair<string, object?>>>(
            standardVals,
            userVals));

    private static IDictionary<string, string> GetPropertyHeaders(
        string alias,
        Func<string, IEnumerable<KeyValuePair<string, string>>> getHeaders)
    {
        IEnumerable<KeyValuePair<string, string>> headers = getHeaders(alias);
        var def = headers.ToDictionary(pt => pt.Key, pt => pt.Value);
        return def;
    }

    private static void PopulateRow(
        this DataTable dt,
        IDictionary<string, string> aliasesToNames,
        IEnumerable<KeyValuePair<string, object?>> standardVals,
        IEnumerable<KeyValuePair<string, object?>> userPropertyVals)
    {
        DataRow dr = dt.NewRow();
        foreach (KeyValuePair<string, object?> r in standardVals)
        {
            dr[r.Key] = r.Value;
        }

        foreach (KeyValuePair<string, object?> p in userPropertyVals.Where(p => p.Value != null))
        {
            dr[aliasesToNames[p.Key]] = p.Value;
        }

        dt.Rows.Add(dr);
    }
}
