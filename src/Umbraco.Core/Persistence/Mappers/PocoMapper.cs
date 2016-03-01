using System;
using System.Reflection;
using NPoco;

namespace Umbraco.Core.Persistence.Mappers
{
    /// <summary>
    /// Represents the PetaPocoMapper, which is the implementation of the IMapper interface.
    /// This is currently only used to ensure that nullable dates are not saved to the database.
    /// </summary>
    public class PocoMapper : DefaultMapper
    {
        public override bool MapMemberToColumn(MemberInfo pi, ref string columnName, ref bool resultColumn)
        {
            return true;
        }

        public override Func<object, object> GetToDbConverter(Type destType, Type sourceType)
        {
            // ensures that NPoco does not try to insert an invalid date
            // from a nullable DateTime property
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
    }
}