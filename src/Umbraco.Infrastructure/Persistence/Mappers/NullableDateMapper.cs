using System.Reflection;
using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
///     Extends NPoco default mapper and ensures that nullable dates are not saved to the database.
/// </summary>
public class NullableDateMapper : DefaultMapper
{
    /// <summary>
    /// Returns a converter function that transforms a nullable <see cref="DateTime"/> value into a database-compatible value, ensuring that invalid dates (such as <see cref="DateTime.MinValue"/>) are not stored.
    /// </summary>
    /// <param name="destType">The type expected by the database column.</param>
    /// <param name="sourceMemberInfo">The reflection metadata for the source member being mapped.</param>
    /// <returns>
    /// A function that takes an object (expected to be a nullable <see cref="DateTime"/>) and returns either a valid <see cref="DateTime"/> value or <c>null</c> if the value is <c>null</c> or <see cref="DateTime.MinValue"/>; returns <c>null</c> if the source member is not a <see cref="DateTime"/>.
    /// </returns>
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
