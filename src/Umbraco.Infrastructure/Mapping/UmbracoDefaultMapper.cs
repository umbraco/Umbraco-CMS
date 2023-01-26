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

        return base.GetFromDbConverter(destType, sourceType);
    }
}
