using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using umbraco.interfaces;
using System.Xml;

namespace umbraco.cms.businesslogic.datatype
{
    /// <summary>
    /// Default implementation of the <c>IData</c> interface that stores data inside the Umbraco database.
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in the future.")]
    public class DefaultData : IData, IDataWithPreview, IDataValueSetter
	{
		private int _propertyId;
		private object _value;
		protected BaseDataType _dataType;
        private bool _previewMode;
        private bool _valueLoaded = false;
		private Guid? _version = null;
		private int? _nodeId = null;

        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        //TODO Refactor this class to use the Database object instead of the SqlHelper
        //NOTE DatabaseContext.Current.Database should eventually be replaced with that from the Repository-Resolver refactor branch. 
        internal static UmbracoDatabase Database
        {
            get { return ApplicationContext.Current.DatabaseContext.Database; }
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
            _propertyId = InitPropertyId;
            _value = InitValue;
        }

        /// <summary>
        /// This is here for performance reasons since in some cases we will have already resolved the value from the db
        /// and want to just give this object the value so it doesn't go re-look it up from the database.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="strDbType"></param>
        void IDataValueSetter.SetValue(object val, string strDbType)
        {
            //We need to ensure that val is not a null value, if it is then we'll convert this to an empty string.
            //The reason for this is because by default the DefaultData.Value property returns an empty string when 
            // there is no value, this is based on the PropertyDataDto.GetValue return value which defaults to an 
            // empty string (which is called from this class's method LoadValueFromDatabase). 
            //Some legacy implementations of DefaultData are expecting an empty string when there is 
            // no value so we need to keep this consistent.
            if (val == null)
            {
                val = string.Empty;
            }

            _value = val;
            //now that we've set our value, we can update our BaseDataType object with the correct values from the db
            //instead of making it query for itself. This is a peformance optimization enhancement.
            var dbType = BaseDataType.GetDBType(strDbType);
            var fieldName = BaseDataType.GetDataFieldName(dbType);

            //if misconfigured (datatype created in the tree, but save button never clicked), the datatype will be null
            if(_dataType != null)
                _dataType.SetDataTypeProperties(fieldName, dbType);

            //ensures that it doesn't go back to the db
            _valueLoaded = true;
        }

        /// <summary>
        /// Loads the data value from the database.
        /// </summary>
        protected internal virtual void LoadValueFromDatabase()
        {
            var sql = new Sql();
            sql.Select("*")
               .From<PropertyDataDto>()
               .InnerJoin<PropertyTypeDto>()
               .On<PropertyTypeDto, PropertyDataDto>(x => x.Id, y => y.PropertyTypeId)
               .InnerJoin<DataTypeDto>()
               .On<DataTypeDto, PropertyTypeDto>(x => x.DataTypeId, y => y.DataTypeId)
               .Where<PropertyDataDto>(x => x.Id == _propertyId);
            var dto = Database.Fetch<PropertyDataDto, PropertyTypeDto, DataTypeDto>(sql).FirstOrDefault();

            if (dto != null && _dataType != null)
            {
                //the type stored in the cmsDataType table
                var strDbType = dto.PropertyTypeDto.DataTypeDto.DbType;
                //get the enum of the data type
                var dbType = BaseDataType.GetDBType(strDbType);
                //get the column name in the cmsPropertyData table that stores the correct information for the data type
                var fieldName = BaseDataType.GetDataFieldName(dbType);
                //get the value for the data type, if null, set it to an empty string
                _value = dto.GetValue;
                //now that we've set our value, we can update our BaseDataType object with the correct values from the db
                //instead of making it query for itself. This is a peformance optimization enhancement.
                _dataType.SetDataTypeProperties(fieldName, dbType);
            }
        }

        internal string PropertyTypeAlias { get; set; }

        public DBTypes DatabaseType
        {
            get
            {
                return _dataType.DBType;
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
			if (_dataType.DBType == DBTypes.Ntext)
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
                if (!_valueLoaded)
                {
                    LoadValueFromDatabase();
                    _valueLoaded = true;
                } 
				return _value;
			}
			set 
			{
                _value = value;
                _valueLoaded = true;
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
				return _propertyId;
			}
			set
			{
				_propertyId = value;
                //LoadValueFromDatabase();
			}
		}
		
		// TODO: clean up Legacy - these are needed by the wysiwyeditor, in order to feed the richtextholder with version and nodeid
		// solution, create a new version of the richtextholder, which does not depend on these.
        public virtual Guid Version
        {
			get
			{
				if (_version == null)
				{
					var dto = Database.FirstOrDefault<PropertyDataDto>("WHERE id = @Id", new { Id = PropertyId });
					_version = dto.VersionId.HasValue ? dto.VersionId.Value : Guid.Empty;	
				}
				return _version.Value;
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
				if (_nodeId == null)
				{
					_nodeId = Database.ExecuteScalar<int>("Select contentNodeid from cmsPropertyData where id = @Id", new { Id = PropertyId });	
				}
				return _nodeId.Value;
			}
            internal set { _nodeId = value; }
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
                return _previewMode;
            }
            set
            {
                if (_previewMode != value)
                {
                    // if preview mode is switched off, reload the value from persistent storage
                    if (!value)
                        LoadValueFromDatabase();
                    _previewMode = value;
                }
            }
        }

        #endregion

        
    }	
}
