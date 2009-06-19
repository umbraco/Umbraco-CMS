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
    /// Any data type in umbraco can have PreValues, which is a simple Key/Value collection of items attached to a specific instance of the data type.
    /// </summary>
    public class PreValues
    {
        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// Gets the pre values collection.
        /// </summary>
        /// <param name="DataTypeId">The data type id.</param>
        /// <returns></returns>
        public static SortedList GetPreValues(int DataTypeId)
        {
            SortedList retval = new SortedList();
            IRecordsReader dr = SqlHelper.ExecuteReader(
                "Select id, sortorder, [value] from cmsDataTypePreValues where DataTypeNodeId = @dataTypeId order by sortorder",
                SqlHelper.CreateParameter("@dataTypeId", DataTypeId));

            int counter = 0;
            while (dr.Read())
            {
                retval.Add(counter, new PreValue(dr.GetInt("id"), dr.GetInt("sortorder"), dr.GetString("value")));
                counter++;
            }
            dr.Close();
            return retval;
        }

    }

    /// <summary>
    /// A simple class for storing predefined values on a datatype.
    /// A prevalue contains a value, a unique key and sort order.
    /// </summary>
    public class PreValue
    {
        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreValue"/> class.
        /// </summary>
        public PreValue()
        {
            
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
                object tempSortOrder = SqlHelper.ExecuteScalar<object>("select max(sortorder) from cmsDataTypePreValues where datatypenodeid = @dataTypeId", SqlHelper.CreateParameter("@dataTypeId", DataTypeId));
                int _sortOrder = 0;

                if (tempSortOrder!=null && int.TryParse(tempSortOrder.ToString(), out _sortOrder))
                        SortOrder = _sortOrder + 1;
                    else
                        SortOrder = 1;

                IParameter[] SqlParams = new IParameter[] {
								SqlHelper.CreateParameter("@value",Value),
								SqlHelper.CreateParameter("@dtdefid",DataTypeId)};
                // The method is synchronized
                SqlHelper.ExecuteNonQuery("INSERT INTO cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) VALUES (@dtdefid,@value,0,'')", SqlParams);
                _id = SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM cmsDataTypePreValues");
            }

            SqlHelper.ExecuteNonQuery(
                "update cmsDataTypePreValues set sortorder = @sortOrder, [value] = @value where id = @id",
                SqlHelper.CreateParameter("@sortOrder", SortOrder),
                SqlHelper.CreateParameter("@value", Value),
                SqlHelper.CreateParameter("@id", Id));
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
            object id = SqlHelper.ExecuteScalar<object>(
                "Select id from cmsDataTypePreValues where [Value] = @value and DataTypeNodeId = @dataTypeId",
                SqlHelper.CreateParameter("@dataTypeId", DataTypeId),
                SqlHelper.CreateParameter("@value", Value));
            if (id != null)
                _id = int.Parse(id.ToString());
            
            initialize();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void initialize()
        {
            IRecordsReader dr = SqlHelper.ExecuteReader(
                 "Select id, sortorder, [value] from cmsDataTypePreValues where id = @id order by sortorder",
                 SqlHelper.CreateParameter("@id", _id));
            if (dr.Read())
            {
                _sortOrder = dr.GetInt("sortorder");
                _value = dr.GetString("value");
            }
            dr.Close();
        }

        private int  _dataTypeId;

        /// <summary>
        /// Gets or sets the data type id.
        /// </summary>
        /// <value>The data type id.</value>
        public int  DataTypeId
        {
            get { return _dataTypeId; }
            set { _dataTypeId = value; }
        }
	

        private int _id;

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
	

        private string  _value;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string  Value
        {
            get { return _value; }
            set { _value = value; }
        }
	

        private int _sortOrder;

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        /// <value>The sort order.</value>
        public int SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }
	
    }
}
