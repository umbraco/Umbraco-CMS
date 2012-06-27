using System;

using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace umbraco.editorControls
{
    [Obsolete("Use umbraco.cms.businesslogic.datatype.DefaultData instead")]
	public class DefaultData : interfaces.IData
	{
		private int _propertyId;
		private object _value;
		protected BaseDataType _dataType;

        public static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }
        
		public DefaultData(BaseDataType DataType) {
			_dataType = DataType;
		}

		#region IData Members

		public virtual System.Xml.XmlNode ToXMl(System.Xml.XmlDocument d)
		{
			if (this._dataType.DBType == DBTypes.Ntext) 
				return d.CreateCDataSection(this.Value.ToString());
			return d.CreateTextNode(Value.ToString());
		}
		
		public object Value 
		{
			get 
			{
				return _value;
			}
			set 
			{
				// Try to set null values if possible
				try 
				{
					if (value == null)
						SqlHelper.ExecuteNonQuery("update cmsPropertyData set "+ _dataType.DataFieldName +" = NULL where id = " + _propertyId);
					else
						SqlHelper.ExecuteNonQuery("update cmsPropertyData set "+ _dataType.DataFieldName +" = @value where id = " + _propertyId, SqlHelper.CreateParameter("@value", value) );
					_value = value;
				} 
				catch (Exception e)
				{
					umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, umbraco.BusinessLogic.User.GetUser(0), -1, "Error updating item: " + e.ToString());
					if (value==null) value ="";
					SqlHelper.ExecuteNonQuery("update cmsPropertyData set "+ _dataType.DataFieldName +" = @value where id = " + _propertyId, SqlHelper.CreateParameter("@value", value) );
					_value = value;
				}

			}
		}

		public virtual void MakeNew(int PropertyId)
		{
			// this default implementation of makenew does not do anything sínce 
			// it uses the default datastorage of umbraco, and the row is already populated by the "property" object
			// If the datatype needs to have a default value, inherit this class and override this method.
		}

		public void Delete() {
			// this default implementation of delete does not do anything sínce 
			// it uses the default datastorage of umbraco, and the row is already deleted by the "property" object
		}

		public int PropertyId
		{
			get {
				return _propertyId;
			}
			set
			{
				_propertyId = value;
				_value = SqlHelper.ExecuteScalar<object>("Select " + _dataType.DataFieldName + " from cmsPropertyData where id = " + value);
			}
		}

		// TODO: clean up Legacy - these are needed by the wysiwyeditor, in order to feed the richtextholder with version and nodeid
		// solution, create a new version of the richtextholder, which does not depend on these.
		public Guid Version {
			get {
				return new Guid(SqlHelper.ExecuteScalar<string>("Select versionId from cmsPropertyData where id = " + PropertyId).ToString());
			}
		}

		public int NodeId {
			get {
				return SqlHelper.ExecuteScalar<int>("Select contentNodeid from cmsPropertyData where id = " + PropertyId);
			}
		}
		#endregion
	}	
}
