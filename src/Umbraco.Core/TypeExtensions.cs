using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core
{
	public static class TypeExtensions
	{
		public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit)
		{
			return type.GetCustomAttributes(typeof(T), inherit).Cast<T>();
		}

		public static T GetCustomAttribute<T>(this Type type, bool inherit)
		{
			return type.GetCustomAttributes<T>(inherit).SingleOrDefault();
		}
	}
}
