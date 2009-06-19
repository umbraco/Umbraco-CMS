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
            m_Value = SqlHelper.ExecuteScalar<object>("SELECT " + _dataType.DataFieldName + " FROM cmsPropertyData WHERE id = " + m_PropertyId);
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
				return m_Value;
			}
			set 
			{
                if (!PreviewMode)
                {
                    // Try to set null values if possible
                    try
                    {
                        if (value == null)
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
                LoadValueFromDatabase();
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
