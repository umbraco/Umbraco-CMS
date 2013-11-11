using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Runtime.CompilerServices;

using umbraco.DataLayer;
using umbraco.BusinessLogic;
using Umbraco.Core.Persistence;
using Umbraco.Core;
using Umbraco.Core.Persistence.DatabaseAnnotations;


namespace umbraco.cms.businesslogic.datatype
{
    /// <summary>
    /// A simple class for storing predefined values on a datatype.
    /// A prevalue contains a value, a unique key and sort order.
    /// </summary>
    public class PreValue
    {
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }
        internal static UmbracoDatabase Database
        {
            get { return ApplicationContext.Current.DatabaseContext.Database; }
        }

        #region Contructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PreValue"/> class.
        /// </summary>
        public PreValue()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreValue"/> class.
        /// </summary>
        /// <param name="Id">The id.</param>
        /// <param name="SortOrder">The sort order.</param>
        /// <param name="Value">The value.</param>
        public PreValue(int Id, int SortOrder, string Value)
        {
            _id = Id;
            _sortOrder = SortOrder;
            _value = Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreValue"/> class.
        /// </summary>
        /// <param name="Id">The id.</param>
        public PreValue(int Id)
        {
            _id = Id;
            initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreValue"/> class.
        /// </summary>
        /// <param name="DataTypeId">The data type id.</param>
        /// <param name="Value">The value.</param>
        public PreValue(int DataTypeId, string Value)
        {
            object id = Database.ExecuteScalar<object>("Select id from cmsDataTypePreValues where [Value] = @value and DataTypeNodeId = @dataTypeId",
                                   new { value = Value, dataTypeId = DataTypeId });
            if (id == null) throw new ArgumentException(string.Format("Can't fetch a PreValue instance from database for DataTypeId = {0} and Value = '{1}'", DataTypeId, Value ));   
                
            _id = (int)id;
            initialize();
        } 
        #endregion

        /// <summary>
        /// Create a new pre value with a value
        /// </summary>
        /// <param name="dataTypeDefId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static PreValue MakeNew(int dataTypeDefId, string value)
        {
            Database.Execute(
                "insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",
                new {dtdefid = dataTypeDefId, value =  value });
            var id = Database.ExecuteScalar<int>("SELECT MAX(id) FROM cmsDataTypePreValues");
            return new PreValue(id);
        }

        #region Private members
        private int _dataTypeId;
        private int? _id;
        private string _value;
        private int _sortOrder; 
        #endregion

        #region Public properties
        /// <summary>
        /// Gets or sets the data type id.
        /// </summary>
        /// <value>The data type id.</value>
        public int DataTypeId
        {
            get { return _dataTypeId; }
            set { _dataTypeId = value; }
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id
        {
            get 
            {
                if (_id == null) throw new InvalidOperationException("ID is null"); 
                return _id.Value; 
            }
            set { _id = value; }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        public int SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        } 
        #endregion

        #region Public methods

        /// <summary>
        /// Deletes a prevalue item
        /// </summary>
        public void Delete()
        {
            if (_id == null) 
            {
                throw new ArgumentNullException("Id is null");
            }

            Database.Execute("delete from cmsDataTypePreValues where id = @0", this.Id); 
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Save()
        {
            // Check for new
            if (Id == 0)
            {
                // Update sortOrder
                object tempSortOrder = Database.ExecuteScalar<object>("select max(sortorder) from cmsDataTypePreValues where datatypenodeid = @0", DataTypeId);
                int _sortOrder = 0;

                if (tempSortOrder != null && int.TryParse(tempSortOrder.ToString(), out _sortOrder))
                    SortOrder = _sortOrder + 1;
                else
                    SortOrder = 1;

                // The method is synchronized
                Database.Execute("INSERT INTO cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) VALUES (@0, @1, 0,'')", DataTypeId, Value);
                _id = Database.ExecuteScalar<int>("SELECT MAX(id) FROM cmsDataTypePreValues");
            }

            Database.Execute(
                "update cmsDataTypePreValues set sortorder = @sortOrder, [value] = @value where id = @id",
                   new { sortOrder =  SortOrder, value = Value, id = Id });
        } 
        
        #endregion

        #region Private methods
        [TableName("cmsDataTypePreValues")]
        [PrimaryKey("id")]
        [ExplicitColumns]
        internal class PreValueDto
        {
            [Column("Id")]
            [PrimaryKeyColumn(IdentitySeed = 1)]
            public int Id { get; set; }
            [Column("SortOrder")]
            public int SortOrder { get; set; }
            [Column("Value")]
            public string Value { get; set; }
            [Column("dataTypeNodeId")]
            public int DataTypeId { get; set; }  // source DataTypeNodeId
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void initialize()
        {
            if (_id == null) throw new ArgumentNullException("Id is null");

            Database.FirstOrDefault<PreValueDto>(
                 "Select id, sortorder, [value], dataTypeNodeId from cmsDataTypePreValues where id = @id order by sortorder",
                 new { id = _id })
            .IfNull<PreValueDto>(x => { throw new ArgumentException(string.Format("Can't fetch a PreValue instance for ID = {0}", _id)); })
            .IfNotNull<PreValueDto>(x =>
            {
                _sortOrder = x.SortOrder;
                _value = x.Value;
                _dataTypeId = x.DataTypeId;
            });
            

        } 
        #endregion
       
    }
}
