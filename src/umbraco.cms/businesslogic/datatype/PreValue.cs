using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Runtime.CompilerServices;

using umbraco.DataLayer;
using umbraco.BusinessLogic;

namespace umbraco.cms.businesslogic.datatype
{
    /// <summary>
    /// A simple class for storing predefined values on a datatype.
    /// A prevalue contains a value, a unique key and sort order.
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in the future.")]
    public class PreValue
    {
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
            this.SortOrder = SortOrder;
            this.Value = Value;
        }

        public PreValue(int id, int sortOrder, string value, string alias)
        {
            _id = id;
            SortOrder = sortOrder;
            Value = value;
            Alias = alias;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreValue"/> class.
        /// </summary>
        /// <param name="Id">The id.</param>
        public PreValue(int Id)
        {
            _id = Id;
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreValue"/> class.
        /// </summary>
        /// <param name="DataTypeId">The data type id.</param>
        /// <param name="Value">The value.</param>
        public PreValue(int DataTypeId, string Value)
        {
            using (var sqlHelper = Application.SqlHelper)
            {
                var id = sqlHelper.ExecuteScalar<object>(
                    "Select id from cmsDataTypePreValues where [Value] = @value and DataTypeNodeId = @dataTypeId",
                    sqlHelper.CreateParameter("@dataTypeId", DataTypeId),
                    sqlHelper.CreateParameter("@value", Value));
                if (id != null)
                    _id = int.Parse(id.ToString());
            }
            Initialize();
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
            using (var sqlHelper = Application.SqlHelper)
            {
                sqlHelper.ExecuteNonQuery(
                    "insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",
                    sqlHelper.CreateParameter("@dtdefid", dataTypeDefId),
                    sqlHelper.CreateParameter("@value", value));
                var id = sqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsDataTypePreValues");
                return new PreValue(id);
            }
        }

        #region Private members

        private int? _id;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets the data type id.
        /// </summary>
        /// <value>The data type id.</value>
        public int DataTypeId { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id
        {
            get { return _id.Value; }
            set { _id = value; }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        public string Alias { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Deletes a prevalue item
        /// </summary>
        public void Delete()
        {
            if (_id == null) 
            {
                throw new ArgumentNullException("Id");
            }

            using (var sqlHelper = Application.SqlHelper)
                sqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where id = @id",
                sqlHelper.CreateParameter("@id", this.Id));
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
                using (var sqlHelper = Application.SqlHelper)
                {
                    var tempSortOrder = sqlHelper.ExecuteScalar<object>("select max(sortorder) from cmsDataTypePreValues where datatypenodeid = @dataTypeId", sqlHelper.CreateParameter("@dataTypeId", DataTypeId));
                    var sortOrder = 0;

                    if (tempSortOrder != null && int.TryParse(tempSortOrder.ToString(), out sortOrder))
                        SortOrder = sortOrder + 1;
                    else
                        SortOrder = 1;
             
                    var sqlParams = new IParameter[] {
								sqlHelper.CreateParameter("@value",Value),
								sqlHelper.CreateParameter("@dtdefid",DataTypeId),
                                sqlHelper.CreateParameter("@alias",Alias)};
                
                    // The method is synchronized
                    sqlHelper.ExecuteNonQuery("INSERT INTO cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) VALUES (@dtdefid,@value,0,'@alias')", sqlParams);
                    _id = sqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsDataTypePreValues");
                }
            }

            using (var sqlHelper = Application.SqlHelper)
                sqlHelper.ExecuteNonQuery(
                "update cmsDataTypePreValues set sortorder = @sortOrder, [value] = @value, alias = @alias where id = @id",
                sqlHelper.CreateParameter("@sortOrder", SortOrder),
                sqlHelper.CreateParameter("@value", Value),
                sqlHelper.CreateParameter("@id", Id),
                sqlHelper.CreateParameter("@alias", Alias));
        } 
        
        #endregion

        #region Private methods
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            using (var sqlHelper = Application.SqlHelper)
            using (var dr = sqlHelper.ExecuteReader(
                "Select id, sortorder, [value], alias from cmsDataTypePreValues where id = @id order by sortorder",
                sqlHelper.CreateParameter("@id", Id))) 
            { 
                if (dr.Read())
                {
                    SortOrder = dr.GetInt("sortorder");
                    Value = dr.GetString("value");
                    Alias = dr.GetString("alias");
                }
            }
        } 
        #endregion
       
    }
}
