using System;
using System.Data;

using umbraco.DataLayer;
using umbraco.BusinessLogic;
using umbraco.interfaces;
using System.Xml;

namespace umbraco.cms.businesslogic.datatype
{
    /// <summary>
    /// Default implementation of the <c>IData</c> interface that stores data inside the Umbraco database.
    /// </summary>
    public class DefaultData : IData, IDataWithPreview
	{
		private int m_PropertyId;
		private object m_Value;
		protected BaseDataType _dataType;
        private bool m_PreviewMode;
        private bool m_ValueLoaded = false;

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultData"/> class.
        /// </summary>
        /// <param name="DataType">Type of the data.</param>
		public DefaultData(BaseDataType DataType)
        {
			_dataType = DataType;
		}

        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <param name="InitValue">The init value.</param>
        /// <param name="InitPropertyId">The init property id.</param>
        public virtual void Initialize(object InitValue, int InitPropertyId)
        {
            m_PropertyId = InitPropertyId;
            m_Value = InitValue;
        }

        /// <summary>
        /// Loads the data value from the database.
        /// </summary>
        protected virtual void LoadValueFromDatabase()
        {

            //this is an optimized version of this query. In one call it will return the data type
            //and the values, this will then set the underlying db type and value of the BaseDataType object
            //instead of having it query the database itself.
            var sql = @"
                SELECT dataInt, dataDate, dataNvarchar, dataNtext, dbType FROM cmsPropertyData 
                INNER JOIN cmsPropertyType ON cmsPropertyType.id = cmsPropertyData.propertytypeid
                INNER JOIN cmsDataType ON cmsDataType.nodeId = cmsPropertyType.dataTypeId
                WHERE cmsPropertyData.id = " + m_PropertyId;

            using (var r = SqlHelper.ExecuteReader(sql))
            {
                if (r.Read())
                {
                    //the type stored in the cmsDataType table
                    var strDbType = r.GetString("dbType"); 
                    //get the enum of the data type
                    var dbType = BaseDataType.GetDBType(strDbType); 
                    //get the column name in the cmsPropertyData table that stores the correct information for the data type
                    var fieldName = BaseDataType.GetDataFieldName(dbType);
                    //get the value for the data type, if null, set it to an empty string
                    m_Value = r.GetObject(fieldName) ?? string.Empty;

                    //now that we've set our value, we can update our BaseDataType object with the correct values from the db
                    //instead of making it query for itself. This is a peformance optimization enhancement.
                    _dataType.SetDataTypeProperties(fieldName, dbType);
                }                
            }
        }

        public DBTypes DatabaseType
        {
            get
            {
                return this._dataType.DBType;
            }
        }

        public int DataTypeDefinitionId
        {
            get
            {
                return _dataType.DataTypeDefinitionId;
            }
        }


		#region IData Members

        /// <summary>
        /// Converts the data to XML.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The data as XML.</returns>
		public virtual XmlNode ToXMl(XmlDocument data)
		{
            string sValue = Value!=null ? Value.ToString() : String.Empty;
			if (this._dataType.DBType == DBTypes.Ntext)
                return data.CreateCDataSection(sValue);
            return data.CreateTextNode(sValue);
		}

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public virtual object Value 
		{
			get 
			{
                //Lazy load the value when it is required.
                if (!m_ValueLoaded)
                {
                    LoadValueFromDatabase();
                    m_ValueLoaded = true;
                } 
				return m_Value;
			}
			set 
			{
                if (!PreviewMode)
                {
                    // Try to set null values if possible
                    try
                    {
                        //CHANGE:by Allan Laustsen to fix copy nodes
                        //if (value == null)
                        if (value == null || (string.IsNullOrEmpty(value.ToString()) && (this._dataType.DBType == DBTypes.Integer || this._dataType.DBType == DBTypes.Date)))
                            SqlHelper.ExecuteNonQuery("update cmsPropertyData set " + _dataType.DataFieldName + " = NULL where id = " + m_PropertyId);
                        else
                            SqlHelper.ExecuteNonQuery("update cmsPropertyData set " + _dataType.DataFieldName + " = @value where id = " + m_PropertyId, SqlHelper.CreateParameter("@value", value));
                    }
                    catch (Exception e)
                    {
                        umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, umbraco.BusinessLogic.User.GetUser(0), -1, "Error updating item: " + e.ToString());
                        if (value == null) value = "";
                        SqlHelper.ExecuteNonQuery("update cmsPropertyData set " + _dataType.DataFieldName + " = @value where id = " + m_PropertyId, SqlHelper.CreateParameter("@value", value));
                    }
                }
                m_Value = value;
			}
		}

        /// <summary>
        /// Creates a new value.
        /// </summary>
        /// <param name="PropertyId">The property id.</param>
		public virtual void MakeNew(int PropertyId)
		{
			// this default implementation of makenew does not do anything sínce 
			// it uses the default datastorage of umbraco, and the row is already populated by the "property" object
			// If the datatype needs to have a default value, inherit this class and override this method.
		}

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public virtual void Delete()
        {
			// this default implementation of delete does not do anything sínce 
			// it uses the default datastorage of umbraco, and the row is already deleted by the "property" object
		}

        /// <summary>
        /// Gets or sets the property id.
        /// </summary>
        /// <value>The property id.</value>
        public virtual int PropertyId
		{
			get
            {
				return m_PropertyId;
			}
			set
			{
				m_PropertyId = value;
                //LoadValueFromDatabase();
			}
		}

		// TODO: clean up Legacy - these are needed by the wysiwyeditor, in order to feed the richtextholder with version and nodeid
		// solution, create a new version of the richtextholder, which does not depend on these.
        public virtual Guid Version
        {
			get
            {
                using (IRecordsReader dr = SqlHelper.ExecuteReader("SELECT versionId FROM cmsPropertyData WHERE id = " + PropertyId))
                {
                    dr.Read();
                    return dr.GetGuid("versionId");
                }
			}
		}

        /// <summary>
        /// Gets the node id.
        /// </summary>
        /// <value>The node id.</value>
        public virtual int NodeId
        {
			get
            {
				return SqlHelper.ExecuteScalar<int>("Select contentNodeid from cmsPropertyData where id = " + PropertyId);
			}
		}

		#endregion

        #region IDataWithPreview Members

        /// <summary>
        /// Gets or sets a value indicating whether preview mode is switched on.
        /// In preview mode, the <see cref="Value"/> setter saves to a temporary location
        /// instead of persistent storage, which the getter also reads from on subsequent access.
        /// Switching off preview mode restores the persistent value.
        /// </summary>
        /// <value><c>true</c> if preview mode is switched on; otherwise, <c>false</c>.</value>
        public virtual bool PreviewMode
        {
            get
            {
                return m_PreviewMode;
            }
            set
            {
                if (m_PreviewMode != value)
                {
                    // if preview mode is switched off, reload the value from persistent storage
                    if (!value)
                        LoadValueFromDatabase();
                    m_PreviewMode = value;
                }
            }
        }

        #endregion
    }	
}
