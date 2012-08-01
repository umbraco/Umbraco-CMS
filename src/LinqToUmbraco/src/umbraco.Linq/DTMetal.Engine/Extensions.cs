using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.DataLayer;
using System.Data.SqlTypes;

namespace umbraco.Linq.DTMetal.Engine
{
    public static class Extensions
    {
        public static string GetAlias(this IRecordsReader reader)
        {
            return reader.GetString("Alias");
        }

        public static string GetName(this IRecordsReader reader)
        {
            return reader.GetString("Name");
        }

        public static int GetId(this IRecordsReader reader)
        {
            return reader.GetInt("Id");
        }

        public static string GetDescription(this IRecordsReader reader)
        {
            return reader.GetString("Description");
        }

        public static string GetDbType(this IRecordsReader reader)
        {
            return reader.GetString("DbType");
        }

        public static int GetParentId(this IRecordsReader reader)
        {
            try
            {
                return reader.GetInt("ParentId");
            }
            catch (SqlNullValueException)
            {
                return 0;
            }
        }
    }
}
