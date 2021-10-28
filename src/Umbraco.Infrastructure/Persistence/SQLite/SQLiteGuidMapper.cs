using System;
using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.SQLite
{
    public class SQLiteGuidMapper : DefaultMapper
    {
        public override Func<object, object> GetFromDbConverter(Type destType, Type sourceType)
        {
            if (destType == typeof(Guid))
            {
                return (value) =>
                {
                    var result = Guid.Parse($"{value}");
                    return result;
                };
            }

            if (destType == typeof(Guid?))
            {
                return (value) =>
                {
                    if (Guid.TryParse($"{value}", out Guid result))
                    {
                        return result;
                    }

                    return null;
                };
            }


            return base.GetFromDbConverter(destType, sourceType);
        }


    }
}
