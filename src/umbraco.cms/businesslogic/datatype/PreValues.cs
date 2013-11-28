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
    [Obsolete("This class is no longer used and will be removed from the codebase in the future.")]
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
            var retval = new SortedList();
            var dr = SqlHelper.ExecuteReader(
                "Select id, sortorder, [value], alias from cmsDataTypePreValues where DataTypeNodeId = @dataTypeId order by sortorder",
                SqlHelper.CreateParameter("@dataTypeId", DataTypeId));

            int counter = 0;
            while (dr.Read())
            {
                retval.Add(counter, new PreValue(dr.GetInt("id"), dr.GetInt("sortorder"), dr.GetString("value"), dr.GetString("alias")));
                counter++;
            }
            dr.Close();
            return retval;
        }

        /// <summary>
        /// Removes all prevalues with the specified data type definition id
        /// </summary>
        /// <param name="dataTypeDefId"></param>
        public static void DeleteByDataTypeDefinition(int dataTypeDefId)
        {
            SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid",
                SqlHelper.CreateParameter("@dtdefid", dataTypeDefId));
        }

        /// <summary>
        /// Returns the number of prevalues for data type definition
        /// </summary>
        /// <param name="dataTypeDefId"></param>
        /// <returns></returns>
        public static int CountOfPreValues(int dataTypeDefId)
        {
            return SqlHelper.ExecuteScalar<int>(
                "select count(id) from cmsDataTypePreValues where dataTypeNodeId = @dataTypeId",
                SqlHelper.CreateParameter("@dataTypeId", dataTypeDefId));
        }

    }

   
}
