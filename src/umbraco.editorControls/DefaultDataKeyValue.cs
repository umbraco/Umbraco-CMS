using System;
using Umbraco.Core.Logging;
using umbraco.DataLayer;

namespace umbraco.editorControls
{
	/// <summary>
	/// Summary description for cms.businesslogic.datatype.DefaultDataKeyValue.
	/// </summary>
    public class DefaultDataKeyValue : cms.businesslogic.datatype.DefaultData
	{
		public DefaultDataKeyValue(cms.businesslogic.datatype.BaseDataType DataType)  : base(DataType)
		{}
		/// <summary>
		/// Ov
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		
		public override System.Xml.XmlNode ToXMl(System.Xml.XmlDocument d)
		{
			// Get the value from 
			var v = "";
			try
			{
			    // Don't query if there's nothing to query for..
			    if (string.IsNullOrWhiteSpace(Value.ToString()) == false)
			    {
			        var dr = SqlHelper.ExecuteReader("Select [value] from cmsDataTypeprevalues where id in (" + SqlHelper.EscapeString(Value.ToString()) + ")");

			        while (dr.Read())
			        {
			            if (v.Length == 0)
			                v += dr.GetString("value");
			            else
			                v += "," + dr.GetString("value");
			        }
			        dr.Close();
			    }
			}
			catch (Exception ex)
			{
			    LogHelper.Error<DefaultDataKeyValue>("An exception occurred converting the property data to XML", ex);
			}
			return d.CreateCDataSection(v);
		}
	}
}
