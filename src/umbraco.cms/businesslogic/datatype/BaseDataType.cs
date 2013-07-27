using System;
using System.Data;

using umbraco.DataLayer;
using umbraco.BusinessLogic;
using System.Collections.Generic;


namespace umbraco.cms.businesslogic.datatype
{
    [Obsolete("This class is no longer used and will be removed from the codebase in the future.")]
	public abstract class BaseDataType
	{
		private int _datatypedefinitionid;
		private string _datafield = "";
		private DBTypes _DBType;

        private static ISqlHelper SqlHelper
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
				 SqlHelper.ExecuteNonQuery("update cmsDataType set  dbType = '" + value.ToString() + "' where nodeId = @datadefinitionid", SqlHelper.CreateParameter("@datadefinitionid",_datatypedefinitionid));
			}
		}
		// Umbraco legacy - get the datafield - the columnname of the cmsPropertyData table
		// where to find the data, since it's configurable - there is no way of telling if
		// its a bit, nvarchar, ntext or datetime field.
		// get it by lookup the value associated to the datatypedefinition id.
		public string DataFieldName 
		{
            get
            {
                if (_datafield == "")
                {
                    string dbtypestr = SqlHelper.ExecuteScalar<string>("select dbType from cmsDataType where nodeId = @datadefinitionid", SqlHelper.CreateParameter("@datadefinitionid", _datatypedefinitionid));
                    DBTypes DataTypeSQLType = GetDBType(dbtypestr);
                    _DBType = DataTypeSQLType;
                    _datafield = GetDataFieldName(_DBType);
                }
                return _datafield;
            }            
		}
		#endregion

        /// <summary>
        /// This is used internally for performance reasons since we are querying for all of the data properties at once in the 
        /// DefaultData object, therefore, the DefaultDataObject will set these properties manually instead of incurring a bunch 
        /// of additional SQL calls.
        /// </summary>
        /// <param name="dataField"></param>
        /// <param name="dataType"></param>
        internal void SetDataTypeProperties(string dataField, DBTypes dataType)
        {
            _datafield = dataField;
            _DBType = dataType;
        }

        /// <summary>
        /// Returns the DBType based on the row value in the dbType column of the cmsDataType
        /// </summary>
        /// <param name="dbtypestr"></param>
        /// <returns></returns>
        internal static DBTypes GetDBType(string dbtypestr)
        {
            return (DBTypes)Enum.Parse(typeof(DBTypes), dbtypestr, true);
        }

        /// <summary>
        /// Returns the data column for the data base where the value resides based on the dbType
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        internal static string GetDataFieldName(DBTypes dbType)
        {
            switch (dbType)
            {
                case DBTypes.Date:
                    return  "dataDate";
                case DBTypes.Integer:
                    return "dataInt";
                case DBTypes.Ntext:
                    return "dataNtext";
                case DBTypes.Nvarchar:
                    return "dataNvarchar";
                default:
                    return "dataNvarchar";
            }
        }

        internal bool HasSettings()
        {
            bool hasSettings = false;
            foreach (System.Reflection.PropertyInfo p in this.GetType().GetProperties())
            {
                object[] o = p.GetCustomAttributes(typeof(DataEditorSetting), true);

                if (o.Length > 0)
                {
                    hasSettings = true;
                    break;
                }
            }

            return hasSettings;
        }

        internal Dictionary<string, DataEditorSetting> Settings()
        {
            Dictionary<string, DataEditorSetting> s = new Dictionary<string, DataEditorSetting>();

            foreach (System.Reflection.PropertyInfo p in this.GetType().GetProperties())
            {

                object[] o = p.GetCustomAttributes(typeof(DataEditorSetting), true);

                if (o.Length > 0)
                    s.Add(p.Name, (DataEditorSetting)o[0]);
            }

            return s;
        }

        internal void LoadSettings(List<Setting<string, string>> settings)
        {
            foreach (Setting<string, string> setting in settings)
            {
                try
                {
                    this.GetType().InvokeMember(setting.Key, System.Reflection.BindingFlags.SetProperty, null, this, new object[] { setting.Value });

                }
                catch (MissingMethodException) { }
            }
        }

	}
}
