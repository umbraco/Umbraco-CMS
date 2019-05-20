using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Umbraco.Core
{
    /// <summary>
    /// Static and extension methods for the DataTable object
    /// </summary>
    internal static class DataTableExtensions
    {
        /// <summary>
        /// Creates a DataTable with the specified alias and columns and uses a callback to populate the headers.
        /// </summary>
        /// <param name="tableAlias"></param>
        /// <param name="getHeaders"></param>
        /// <param name="rowData"> </param>
        /// <returns></returns>
        /// <remarks>
        /// This has been migrated from the Node class and uses proper locking now. It is now used by the Node class and the
        /// DynamicPublishedContent extensions for legacy reasons.
        /// </remarks>
        public static DataTable GenerateDataTable(
            string tableAlias,
            Func<string, IEnumerable<KeyValuePair<string, string>>> getHeaders,
            Func<IEnumerable<Tuple<IEnumerable<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>>>> rowData)
        {
            var dt = new DataTable(tableAlias);

            //get all row data
            var tableData = rowData().ToArray();

            //get all headers
            var propertyHeaders = GetPropertyHeaders(tableAlias, getHeaders);
            foreach(var h in propertyHeaders)
            {
                dt.Columns.Add(new DataColumn(h.Value));
            }

            //add row data
            foreach(var r in tableData)
            {
                dt.PopulateRow(
                    propertyHeaders,
                    r.Item1,
                    r.Item2);
            }

            return dt;
        }

        /// <summary>
        /// Helper method to return this ugly object
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is for legacy code, I didn't want to go creating custom classes for these
        /// </remarks>
        internal static List<System.Tuple<IEnumerable<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>>> CreateTableData()
        {
            return new List<System.Tuple<IEnumerable<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>>>();
        }

        /// <summary>
        /// Helper method to deal with these ugly objects
        /// </summary>
        /// <param name="rowData"></param>
        /// <param name="standardVals"></param>
        /// <param name="userVals"></param>
        /// <remarks>
        /// This is for legacy code, I didn't want to go creating custom classes for these
        /// </remarks>
        internal static void AddRowData(
            List<System.Tuple<IEnumerable<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>>> rowData,
            IEnumerable<KeyValuePair<string, object>> standardVals,
            IEnumerable<KeyValuePair<string, object>> userVals)
        {
            rowData.Add(new System.Tuple<IEnumerable<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>>(
                            standardVals,
                            userVals
                            ));
        }

        private static IDictionary<string, string> GetPropertyHeaders(string alias, Func<string, IEnumerable<KeyValuePair<string, string>>> getHeaders)
        {
            var headers = getHeaders(alias);
            var def = headers.ToDictionary(pt => pt.Key, pt => pt.Value);
            return def;
        }

        private static void PopulateRow(
            this DataTable dt,
            IDictionary<string, string> aliasesToNames,
            IEnumerable<KeyValuePair<string, object>> standardVals,
            IEnumerable<KeyValuePair<string, object>> userPropertyVals)
        {
            var dr = dt.NewRow();
            foreach (var r in standardVals)
            {
                dr[r.Key] = r.Value;
            }
            foreach (var p in userPropertyVals.Where(p => p.Value != null))
            {
                dr[aliasesToNames[p.Key]] = p.Value;
            }
            dt.Rows.Add(dr);
        }

    }
}
