using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
		
		private static readonly ConcurrentDictionary<Type, FieldInfo[]> GetFieldsCache = new ConcurrentDictionary<Type, FieldInfo[]>();
		private static readonly ConcurrentDictionary<Tuple<Type, bool, bool, bool>, PropertyInfo[]> GetPropertiesCache = new ConcurrentDictionary<Tuple<Type, bool, bool, bool>, PropertyInfo[]>();
        
        /// <summary>
        /// Checks if the method is actually overriding a base method
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static bool IsOverride(MethodInfo m)
        {
            return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
        }

        /// <summary>
        /// Find all assembly references that are referencing the assignTypeFrom Type's assembly found in the assemblyList
        /// </summary>
        /// <param name="assignTypeFrom"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the assembly of the assignTypeFrom Type is in the App_Code assembly, then we return nothing since things cannot
        /// reference that assembly, same with the global.asax assembly.
        /// </remarks>
        public static Assembly[] GetReferencedAssemblies(Type assignTypeFrom, IEnumerable<Assembly> assemblies)
        {
            //check if it is the app_code assembly.
            //check if it is App_global.asax assembly
            if (assignTypeFrom.Assembly.IsAppCodeAssembly() || assignTypeFrom.Assembly.IsGlobalAsaxAssembly())
            {
                return Enumerable.Empty<Assembly>().ToArray();
            }
            
            //find all assembly references that are referencing the current type's assembly since we 
            //should only be scanning those assemblies because any other assembly will definitely not
            //contain sub type's of the one we're currently looking for
            return assemblies
                .Where(assembly =>
                       assembly == assignTypeFrom.Assembly || HasReferenceToAssemblyWithName(assembly, assignTypeFrom.Assembly.GetName().Name))
                .ToArray();
        }

        /// <summary>
        /// checks if the assembly has a reference with the same name as the expected assembly name.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="expectedAssemblyName"></param>
        /// <returns></returns>
        private static bool HasReferenceToAssemblyWithName(Assembly assembly, string expectedAssemblyName)
        {
            return assembly
                .GetReferencedAssemblies()
                .Select(a => a.Name)
                .Contains(expectedAssemblyName, StringComparer.Ordinal);
        }

        /// <summary>
        /// Returns true if the type is a class and is not static
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
	    public static bool IsNonStaticClass(Type t)
        {
            return t.IsClass && IsStaticClass(t) == false;
        }

	    /// <summary>
        /// Returns true if the type is a static class
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// In IL a static class is abstract and sealed
        /// see: http://stackoverflow.com/questions/1175888/determine-if-a-type-is-static
        /// </remarks>
        public static bool IsStaticClass(Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

	    /// <summary>
        /// Finds a lowest base class amongst a collection of types
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        /// <remarks>
        /// The term 'lowest' refers to the most base class of the type collection.
        /// If a base type is not found amongst the type collection then an invalid attempt is returned.
        /// </remarks>
        public static Attempt<Type> GetLowestBaseType(params Type[] types)
	    {
	        if (types.Length == 0)
	        {
	            return Attempt<Type>.Fail();
	        }
	        if (types.Length == 1)
	        {
                return Attempt.Succeed(types[0]);
	        }

	        foreach (var curr in types)
	        {
	            var others = types.Except(new[] {curr});

	            //is the curr type a common denominator for all others ?
	            var isBase = others.All(curr.IsAssignableFrom);

	            //if this type is the base for all others
	            if (isBase)
	            {
	                return Attempt.Succeed(curr);
	            }
	        }

	        return Attempt<Type>.Fail();
	    }

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
		    return contract.IsAssignableFrom(implementation);
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
		    return implementation.IsValueType || implementation.IsPrimitive;
		}

		/// <summary>
		/// A cached method to determine whether <paramref name="implementation"/> is an implied value type (<see cref="Type.IsValueType"/>, <see cref="Type.IsEnum"/> or a string).
		/// </summary>
		/// <param name="implementation">The implementation.</param>
		public static bool IsImplicitValueType(Type implementation)
		{
		    return IsValueType(implementation) || implementation.IsEnum || implementation == typeof (string);
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
        /// Returns all public properties including inherited properties even for interfaces
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// taken from http://stackoverflow.com/questions/358835/getproperties-to-return-all-properties-for-an-interface-inheritance-hierarchy
        /// </remarks>
        public static PropertyInfo[] GetPublicProperties(Type type)
        {
            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Public
                        | BindingFlags.Instance);

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetProperties(BindingFlags.FlattenHierarchy
                | BindingFlags.Public | BindingFlags.Instance);
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