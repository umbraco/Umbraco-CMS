using System;

namespace umbraco.editorControls.datepicker
{
	/// <summary>
	/// Summary description for DateData.
	/// </summary>
	public class DateData : cms.businesslogic.datatype.DefaultData
	{
		public DateData(cms.businesslogic.datatype.BaseDataType DataType) : base(DataType) {}

		public override System.Xml.XmlNode ToXMl(System.Xml.XmlDocument d)
		{
			if (Value.ToString() != "")
				return d.CreateTextNode(((DateTime) Value).ToString("s"));
			else
				return d.CreateTextNode("");
		}

		public override void MakeNew(int PropertyId) 
		{
			this.PropertyId = PropertyId;
			try 
			{
// Changed, do not insert todays date as default!
//				System.Data.SqlTypes.SqlDateTime sqlDate = new System.Data.SqlTypes.SqlDateTime(DateTime.Now);
//				this.Value = sqlDate;
			} 
			catch 
			{
				this.Value = "";
			}
		}
	}
}
