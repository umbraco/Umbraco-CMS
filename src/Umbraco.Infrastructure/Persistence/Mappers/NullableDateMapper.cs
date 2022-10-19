using System.Reflection;
using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Extends NPoco default mapper and ensures that nullable dates are not saved to the database.
/// </summary>
public class NullableDateMapper : DefaultMapper
{
    public override Func<object, object?>? GetToDbConverter(Type destType, MemberInfo sourceMemberInfo)
    {
        // ensures that NPoco does not try to insert an invalid date
        // from a nullable DateTime property
        if (sourceMemberInfo.GetMemberInfoType() == typeof(DateTime))
        {
            return datetimeVal =>
            {
                var datetime = datetimeVal as DateTime?;
                if (datetime.HasValue && datetime.Value > DateTime.MinValue)
                {
                    return datetime.Value;
                }

                return null;
            };
        }

        return null;
    }
}
