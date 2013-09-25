using System;

namespace umbraco.editorControls.numberfield
{
	/// <summary>
	/// Summary description for DataInteger.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
	public class DataInteger : cms.businesslogic.datatype.DefaultData
	{
		public DataInteger(cms.businesslogic.datatype.BaseDataType DataType) : base(DataType) {}

		public override void MakeNew(int PropertyId) 
        {
			this.PropertyId = PropertyId;
		    string defaultValue = ((DefaultPrevalueEditor) _dataType.PrevalueEditor).Prevalue;
            
            if (defaultValue.Trim() != "")
            {
                this.Value = defaultValue;
            }
		}
	} 
}
