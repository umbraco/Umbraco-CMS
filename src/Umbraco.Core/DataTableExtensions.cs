using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using umbraco.interfaces;

namespace Umbraco.Core
{
	

	/// <summary>
	/// Static and extension methods for the DataTable object
	/// </summary>
	internal static class DataTableExtensions
	{

		private static readonly Hashtable AliasToNames = new Hashtable();
		private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

		/// <summary>
		/// Creates a DataTable with the specified alias and columns and uses a callback to populate the headers.
		/// </summary>
		/// <param name="alias"></param>
		/// <param name="getHeaders"></param>
		/// <param name="rowData"> </param>
		/// <returns></returns>
		/// <remarks>
		/// This has been migrated from the Node class and uses proper locking now. It is now used by the Node class and the 
		/// DynamicDocument extensions for legacy reasons.
		/// </remarks>
		public static DataTable GenerateDataTable(
			string alias, 						
			Func<string, IEnumerable<KeyValuePair<string, string>>> getHeaders,
			Func<IEnumerable<Tuple<IEnumerable<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>>>> rowData)
		{			
			var dt = new DataTable(alias);
			
			//get the standard column headers from the standard data (not user data)
			var tableData = rowData();
			var standardColHeaders = tableData.SelectMany(x => x.Item1).Select(x => x.Key).Distinct();

			var userPropColHeaders = new List<string>();
			// get user property column headers
			var propertyHeaders = GetPropertyHeaders(alias, getHeaders);
			var ide = propertyHeaders.GetEnumerator();
			while (ide.MoveNext())
			{
				userPropColHeaders.Add(ide.Value.ToString());
			}
			
			//now add all the columns, standard val headers first, then user val headers
			foreach (var dc in standardColHeaders.Union(userPropColHeaders).Select(c => new DataColumn(c)))
			{
				dt.Columns.Add(dc);
			}

			//add row data
			foreach(var r in rowData())
			{
				dt.PopulateRow(
					(Hashtable)AliasToNames[alias],
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

		private static Hashtable GetPropertyHeaders(string alias, Func<string, IEnumerable<KeyValuePair<string, string>>> getHeaders)
		{
			using (var l = new UpgradeableReadLock(Lock))
			{
				if (AliasToNames.ContainsKey(alias))
					return (Hashtable)AliasToNames[alias];
				
				l.UpgradeToWriteLock();
				
				var headers = getHeaders(alias);
				var def = new Hashtable();
				foreach (var pt in headers)
					def.Add(pt.Key, pt.Value);
				AliasToNames.Add(alias, def);

				return def;
			}
			
		}

		private static void PopulateRow(
			this DataTable dt, 
			IDictionary aliasesToNames, 
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
				dr[aliasesToNames[p.Key].ToString()] = p.Value;
			}
			dt.Rows.Add(dr);
		}

	}
}
