using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Runtime.CompilerServices;

using umbraco.DataLayer;
using umbraco.BusinessLogic;
using Umbraco.Core.Persistence;
using Umbraco.Core;


namespace umbraco.cms.businesslogic.datatype
{
    /// <summary>
    /// Any data type in umbraco can have PreValues, which is a simple Key/Value collection of items attached to a specific instance of the data type.
    /// </summary>
    public class PreValues
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

        private class preValueDto
        {
            public int id { get; set; }
            public int sortorder { get; set; }
            public string value { get; set; }
        }

        /// <summary>
        /// Gets the pre values collection.
        /// </summary>
        /// <param name="DataTypeId">The data type id.</param>
        /// <returns></returns>
        public static SortedList GetPreValues(int DataTypeId)
        {
            SortedList retval = new SortedList();
            int counter = 0;
            foreach (var preValue in Database.Query<preValueDto>("Select id, sortorder, [value] from cmsDataTypePreValues where DataTypeNodeId = @dataTypeId order by sortorder", new { dataTypeId = DataTypeId }))
            {
                retval.Add(counter, new PreValue(preValue.id, preValue.sortorder, preValue.value)); 
                counter++;
            }
            return retval;
        }

        /// <summary>
        /// Removes all prevalues with the specified data type definition id
        /// </summary>
        /// <param name="dataTypeDefId"></param>
        public static void DeleteByDataTypeDefinition(int dataTypeDefId)
        {
            Database.Execute("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid", new { dtdefid = dataTypeDefId});
        }

        /// <summary>
        /// Returns the number of prevalues for data type definition
        /// </summary>
        /// <param name="dataTypeDefId"></param>
        /// <returns></returns>
        public static int CountOfPreValues(int dataTypeDefId)
        {
            return Database.ExecuteScalar<int>(
                "select count(id) from cmsDataTypePreValues where dataTypeNodeId = @dataTypeId", new { dataTypeId = dataTypeDefId });
        }

    }

   
}
