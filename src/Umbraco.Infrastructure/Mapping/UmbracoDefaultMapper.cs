using System.Globalization;
using NPoco;

namespace Umbraco.Cms.Core.Mapping;

public class UmbracoDefaultMapper : DefaultMapper
{
    public override Func<object, object?> GetFromDbConverter(Type destType, Type sourceType)
    {
        if (destType == typeof(decimal))
        {
            return value =>
            {
                var result = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                return result;
            };
        }

        if (destType == typeof(decimal?))
        {
            return value =>
            {
                var result = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                return result;
            };
        }

        if (destType == typeof(DateOnly))
        {
            return value =>
            {
                return DateOnly.Parse(value.ToString()!);
            };
        }

        if(destType == typeof(TimeOnly))
        {
            return value =>
            {
                return TimeOnly.Parse(value.ToString()!);
            };
        }
        return base.GetFromDbConverter(destType, sourceType);
    }
}
