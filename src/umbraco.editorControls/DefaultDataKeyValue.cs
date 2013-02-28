using System;
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
			string v = "";
			try 
			{
                // Don't query if there's nothing to query for..
                if (string.IsNullOrWhiteSpace(Value.ToString()) == false)
                {
                    IRecordsReader dr = SqlHelper.ExecuteReader("Select [value] from cmsDataTypeprevalues where id in (@id)", SqlHelper.CreateParameter("id", Value.ToString()));

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
			catch {}
			return d.CreateCDataSection(v);
		}
	}
}
