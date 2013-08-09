using System;
using System.ComponentModel;
using System.Reflection;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents the PetaPocoMapper, which is the implementation of the IMapper interface.
    /// This is currently only used to ensure that nullable dates are not saved to the database.
    /// </summary>
    public class PetaPocoMapper : IMapper2
    {
        public void GetTableInfo(Type t, TableInfo ti)
        {
        }

        public bool MapPropertyToColumn(Type t, PropertyInfo pi, ref string columnName, ref bool resultColumn)
        {
            return true;
        }

        public Func<object, object> GetFromDbConverter(PropertyInfo pi, Type sourceType)
        {
            return null;
        }

        public Func<object, object> GetToDbConverter(Type sourceType)
        {
            //We need this check to ensure that PetaPoco doesn't try to insert an invalid 
            //date from a nullable DateTime property.
            if (sourceType == typeof(DateTime))
            {
                return datetimeVal =>
                {
                    var datetime = datetimeVal as DateTime?;
                    if (datetime.HasValue && datetime.Value > DateTime.MinValue)
                        return datetime.Value;

                    return null;
                };
            }

            return null;
        }

        public Func<object, object> GetFromDbConverter(Type DestType, Type SourceType)
        {
            if (SqlSyntaxContext.SqlSyntaxProvider is SqliteSyntaxProvider)
            {
                return src =>
                       {
                           var t = Nullable.GetUnderlyingType(DestType) ?? DestType;

                           if (src is string && t == typeof (Guid))
                               return TypeDescriptor.GetConverter(t).ConvertFromInvariantString(src.ToString());

                           return Convert.ChangeType(src, t, null);
                       };
            }

            return null;
        }
    }
}