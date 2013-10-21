using System;

using umbraco.DataLayer;
using umbraco.BusinessLogic;

namespace umbraco.editorControls
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public abstract class BaseDataType
	{
		private int _datatypedefinitionid;
		private string _datafield = "";
		private DBTypes _DBType;

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

		public BaseDataType()
		{}

		#region IDataType Members
		public abstract  Guid Id {get;}
		public abstract string DataTypeName{get;}
		public abstract interfaces.IDataEditor DataEditor{get;}
		public abstract interfaces.IDataPrevalue PrevalueEditor {get;}
		public abstract interfaces.IData Data{get;}
		
		public int DataTypeDefinitionId {
			set {
				_datatypedefinitionid = value;
			}
			get {
			return _datatypedefinitionid;
			}
		}

		public DBTypes DBType {
			get {
				string test= "";
				if (_datafield == "")
					test = DataFieldName;
				return _DBType;
			}
			set {
				_DBType = value;
				 SqlHelper.ExecuteNonQuery("update cmsDataType set dbType = '" + value.ToString() + "' where nodeId = @datadefinitionid",
                     SqlHelper.CreateParameter("@datadefinitionid",_datatypedefinitionid)).ToString();
				
			}
		}
		// Umbraco legacy - get the datafield - the columnname of the cmsPropertyData table
		// where to find the data, since it's configurable - there is no way of telling if
		// its a bit, nvarchar, ntext or datetime field.
		// get it by lookup the value associated to the datatypedefinition id.
		public string DataFieldName 
		{
			get {
				if (_datafield == "") 
				{
					string dbtypestr = SqlHelper.ExecuteScalar<string>("select dbType from cmsDataType where nodeId = @datadefinitionid",
                        SqlHelper.CreateParameter("@datadefinitionid",_datatypedefinitionid));
					DBTypes DataTypeSQLType = (DBTypes) Enum.Parse(typeof(DBTypes),dbtypestr,true);
					_DBType = DataTypeSQLType;
					switch (DataTypeSQLType) 
					{
						case DBTypes.Date :
							_datafield = "dataDate";
							break;
						case DBTypes.Integer :
							_datafield = "DataInt";
							break;
						case DBTypes.Ntext :
							_datafield = "dataNtext";
							break;
						case DBTypes.Nvarchar :
							_datafield = "dataNvarchar";
							break;
					}
					return _datafield;
				}
				return _datafield;
			}
		}
		#endregion
	}
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public enum DBTypes 
	{
		Integer,
		Date,
		Nvarchar,
		Ntext
	}
}
