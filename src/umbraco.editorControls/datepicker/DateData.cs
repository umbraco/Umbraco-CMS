using System;

namespace umbraco.editorControls.datepicker
{
	/// <summary>
	/// Summary description for DateData.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class DateData : cms.businesslogic.datatype.DefaultData
	{
		public DateData(cms.businesslogic.datatype.BaseDataType DataType) : base(DataType) {}

		public override System.Xml.XmlNode ToXMl(System.Xml.XmlDocument d)
		{
		    if (Value != null && Value.ToString() != "")
		    {
                if(Value is DateTime)
		            return d.CreateTextNode(((DateTime) Value).ToString("s"));

		        DateTime convertedDate;
                if (DateTime.TryParse(Value.ToString(), out convertedDate))
                    return d.CreateTextNode(convertedDate.ToString("s"));
		    }
            
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
