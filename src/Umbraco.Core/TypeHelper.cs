using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Umbraco.Core
{
	/// <summary>
	/// A utility class for type checking, this provides internal caching so that calls to these methods will be faster
	/// than doing a manual type check in c#
	/// </summary>
	internal static class TypeHelper
	{
		private static readonly ConcurrentDictionary<Tuple<Type, Type>, bool> TypeCheckCache = new ConcurrentDictionary<Tuple<Type, Type>, bool>();
		private static readonly ConcurrentDictionary<Type, bool> ValueTypeCache = new ConcurrentDictionary<Type, bool>();
		private static readonly ConcurrentDictionary<Type, bool> ImplicitValueTypeCache = new ConcurrentDictionary<Type, bool>();
		private static readonly ConcurrentDictionary<Type, FieldInfo[]> GetFieldsCache = new ConcurrentDictionary<Type, FieldInfo[]>();
		private static readonly ConcurrentDictionary<Tuple<Type, bool, bool, bool>, PropertyInfo[]> GetPropertiesCache = new ConcurrentDictionary<Tuple<Type, bool, bool, bool>, PropertyInfo[]>();

		/// <summary>
		/// Determines whether the type <paramref name="implementation"/> is assignable from the specified implementation <typeparamref name="TContract"/>,
		/// and caches the result across the application using a <see cref="ConcurrentDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <param name="contract">The type of the contract.</param>
		/// <param name="implementation">The implementation.</param>
		/// <returns>
		/// 	<c>true</c> if [is type assignable from] [the specified contract]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsTypeAssignableFrom(Type contract, Type implementation)
		{
			// NOTE The use of a Tuple<,> here is because its Equals / GetHashCode implementation is literally 10.5x faster than KeyValuePair<,>
			return TypeCheckCache.GetOrAdd(new Tuple<Type, Type>(contract, implementation), x => x.Item1.IsAssignableFrom(x.Item2));
		}

		/// <summary>
		/// Determines whether the type <paramref name="implementation"/> is assignable from the specified implementation <typeparamref name="TContract"/>,
		/// and caches the result across the application using a <see cref="ConcurrentDictionary{TKey,TValue}"/>.
		/// </summary>
		/// <typeparam name="TContract">The type of the contract.</typeparam>
		/// <param name="implementation">The implementation.</param>
		public static bool IsTypeAssignableFrom<TContract>(Type implementation)
		{
			return IsTypeAssignableFrom(typeof(TContract), implementation);
		}

		/// <summary>
		/// A cached method to determine whether <paramref name="implementation"/> represents a value type.
		/// </summary>
		/// <param name="implementation">The implementation.</param>
		public static bool IsValueType(Type implementation)
		{
			return ValueTypeCache.GetOrAdd(implementation, x => x.IsValueType || x.IsPrimitive);
		}

		/// <summary>
		/// A cached method to determine whether <paramref name="implementation"/> is an implied value type (<see cref="Type.IsValueType"/>, <see cref="Type.IsEnum"/> or a string).
		/// </summary>
		/// <param name="implementation">The implementation.</param>
		public static bool IsImplicitValueType(Type implementation)
		{
			return ImplicitValueTypeCache.GetOrAdd(implementation, x => IsValueType(implementation) || implementation.IsEnum || implementation == typeof(string));
		}

		public static bool IsTypeAssignableFrom<TContract>(object implementation)
		{
			if (implementation == null) throw new ArgumentNullException("implementation");
			return IsTypeAssignableFrom<TContract>(implementation.GetType());
		}

		/// <summary>
		/// Returns a PropertyInfo from a type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="mustRead"></param>
		/// <param name="mustWrite"></param>
		/// <param name="includeIndexed"></param>
		/// <param name="caseSensitive"> </param>
		/// <returns></returns>
		public static PropertyInfo GetProperty(Type type, string name, 
			bool mustRead = true, 
			bool mustWrite = true, 
			bool includeIndexed = false,
			bool caseSensitive = true)
		{
			return CachedDiscoverableProperties(type, mustRead, mustWrite, includeIndexed)
				.FirstOrDefault(x =>
					{
						if (caseSensitive)
							return x.Name == name;
						return x.Name.InvariantEquals(name);
					});
		}

		/// <summary>
		/// Gets (and caches) <see cref="FieldInfo"/> discoverable in the current <see cref="AppDomain"/> for a given <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The source.</param>
		/// <returns></returns>
		public static FieldInfo[] CachedDiscoverableFields(Type type)
		{
			return GetFieldsCache.GetOrAdd(
				type,
				x => type
				     	.GetFields(BindingFlags.Public | BindingFlags.Instance)
				     	.Where(y => !y.IsInitOnly)
				     	.ToArray());
		}

		/// <summary>
		/// Gets (and caches) <see cref="PropertyInfo"/> discoverable in the current <see cref="AppDomain"/> for a given <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The source.</param>
		/// <param name="mustRead">true if the properties discovered are readable</param>
		/// <param name="mustWrite">true if the properties discovered are writable</param>
		/// <param name="includeIndexed">true if the properties discovered are indexable</param>
		/// <returns></returns>
		public static PropertyInfo[] CachedDiscoverableProperties(Type type, bool mustRead = true, bool mustWrite = true, bool includeIndexed = false)
		{
			return GetPropertiesCache.GetOrAdd(
				new Tuple<Type, bool, bool, bool>(type, mustRead, mustWrite, includeIndexed),
				x => type
				     	.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				     	.Where(y => (!mustRead || y.CanRead)
				     	            && (!mustWrite || y.CanWrite)
				     	            && (includeIndexed || !y.GetIndexParameters().Any()))
				     	.ToArray());
		}
	}
}