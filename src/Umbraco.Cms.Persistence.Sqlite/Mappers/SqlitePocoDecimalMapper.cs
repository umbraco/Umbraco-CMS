﻿using System.Globalization;
using NPoco;

namespace Umbraco.Cms.Persistence.Sqlite.Mappers;

public class SqlitePocoDecimalMapper : DefaultMapper
{
    public override Func<object, object?> GetFromDbConverter(Type destType, Type sourceType)
    {
        if (destType == typeof(decimal))
        {
            return value =>
            {
                var result = Convert.ToDecimal(value, CultureInfo.CurrentCulture);
                return result;
            };
        }

        if (destType == typeof(decimal?))
        {
            return value =>
            {
                var result = Convert.ToDecimal(value, CultureInfo.CurrentCulture);
                return result;
            };
        }

        return base.GetFromDbConverter(destType, sourceType);
    }
}
