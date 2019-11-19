using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core
{
    public static class TypeExtensions
    {
        public static T GetCustomAttribute<T>(this Type type, bool inherit)
            where T : Attribute
        {
            return type.GetCustomAttributes<T>(inherit).SingleOrDefault();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherited)
            where T : Attribute
        {
            if (type == null) return Enumerable.Empty<T>();
            return type.GetCustomAttributes(typeof (T), inherited).OfType<T>();
        }

    }
}
