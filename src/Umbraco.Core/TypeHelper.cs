using System;
using System.Collections;
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
                       assembly == assignTypeFrom.Assembly 
                        || HasReferenceToAssemblyWithName(assembly, assignTypeFrom.Assembly.GetName().Name))
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
        /// Checks if the generic type passed in can be assigned from the given type
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="implementation"></param>
        /// <returns>
        /// Returns an Attempt{Type} which if true will include the actual type that matched the genericType
        /// being compared.
        /// </returns>
        /// <remarks>
        /// First we need to check a special case, if the generic type is a generic definition but has no FullName, then
        /// we cannot compare it with traditional means because the types will never match. 
        /// Generic types will not have a FullName in these cases: http://blogs.msdn.com/b/haibo_luo/archive/2006/02/17/534480.aspx
        /// or when you retrieve a generic method parameter using reflection, for example, typeof(IEnumerable{}) is not IEnumerable{T} since
        /// when reflected 'T' is actually something.
        ///  
        /// This is using a version modified from: http://stackoverflow.com/a/1075059/1968
        /// </remarks>
        public static Attempt<Type> IsAssignableFromGeneric(Type contract, Type implementation)
        {
            if (contract.IsGenericTypeDefinition == false && contract.FullName.IsNullOrWhiteSpace())
            {
                return IsTypeAssignableFromReflectedGeneric(contract, implementation);
            }

            var genericTypeDef = implementation.IsGenericType ? implementation.GetGenericTypeDefinition() : null;

            if (genericTypeDef != null && genericTypeDef == contract)
                return Attempt<Type>.Succeed(implementation);

            var its = implementation.GetInterfaces();

            foreach (var it in its)
            {
                genericTypeDef = it.IsGenericType ? it.GetGenericTypeDefinition() : null;

                if (genericTypeDef != null && genericTypeDef == contract)
                    return Attempt<Type>.Succeed(it);
            }

            var baseType = implementation.BaseType;
            return baseType != null
                ? IsAssignableFromGeneric(contract, baseType)
                : Attempt<Type>.Fail();
        }

        /// <summary>
        /// This is used in IsAssignableToGenericType
        /// </summary>
        /// <param name="contract">The generic type contract</param>
        /// <param name="implementation"></param>
        /// <returns>
        /// Returns an Attempt{Type} which if true will include the actual type that matched the genericType
        /// being compared.
        /// </returns>
        /// <remarks>
        /// See remarks in method IsAssignableFromGeneric
        /// </remarks>
        private static Attempt<Type> IsTypeAssignableFromReflectedGeneric(Type contract, Type implementation)
        {
            if (implementation.IsGenericType && implementation.GetGenericTypeDefinition().Name == contract.Name && implementation.GenericTypeArguments.Length == contract.GenericTypeArguments.Length)
                return Attempt<Type>.Succeed(implementation);

            var its = implementation.GetInterfaces();

            foreach (var it in its)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition().Name == contract.Name && implementation.GenericTypeArguments.Length == contract.GenericTypeArguments.Length)
                {
                    return Attempt<Type>.Succeed(it);
                }
            }

            var baseType = implementation.BaseType;
            return baseType != null 
                ? IsTypeAssignableFromReflectedGeneric(contract, baseType) 
                : Attempt<Type>.Fail();
        }

        /// <summary>
		/// Determines whether the type <paramref name="implementation"/> is assignable from the specified implementation,
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
        /// Determines whether the object instance <paramref name="implementation"/> is assignable from the specified implementation <typeparamref name="TContract"/>,
        /// and caches the result across the application using a <see cref="ConcurrentDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <typeparam name="TContract">The type of the contract.</typeparam>
        /// <param name="implementation">The implementation.</param>
        public static bool IsTypeAssignableFrom<TContract>(object implementation)
        {
            if (implementation == null) throw new ArgumentNullException("implementation");
            return IsTypeAssignableFrom<TContract>(implementation.GetType());
        }

		/// <summary>
		/// A method to determine whether <paramref name="implementation"/> represents a value type.
		/// </summary>
		/// <param name="implementation">The implementation.</param>
		public static bool IsValueType(Type implementation)
		{
		    return implementation.IsValueType || implementation.IsPrimitive;
		}

		/// <summary>
		/// A method to determine whether <paramref name="implementation"/> is an implied value type (<see cref="Type.IsValueType"/>, <see cref="Type.IsEnum"/> or a string).
		/// </summary>
		/// <param name="implementation">The implementation.</param>
		public static bool IsImplicitValueType(Type implementation)
		{
		    return IsValueType(implementation) || implementation.IsEnum || implementation == typeof (string);
		}

		/// <summary>
		/// Returns (and caches) a PropertyInfo from a type
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


        #region Match Type

        //TODO: Need to determine if these methods should replace/combine/merge etc with IsTypeAssignableFrom, IsAssignableFromGeneric

        private static void ReduceGenericParameterCandidateTypes(ICollection<Type> allStuff, Type type)
        {
            var at1 = new List<Type>();
            var t = type;
            while (t != null)
            {
                at1.Add(t);
                t = t.BaseType;
            }
            var r = allStuff.Where(x => x.IsClass && at1.Contains(x) == false).ToArray();
            foreach (var x in r) allStuff.Remove(x);
            var ai1 = type.GetInterfaces();
            if (type.IsInterface) ai1 = ai1.Union(new[] { type }).ToArray();
            r = allStuff.Where(x => x.IsInterface && ai1.Contains(x) == false).ToArray();
            foreach (var x in r) allStuff.Remove(x);
        }

        private static bool MatchGeneric(Type inst, Type type, IDictionary<string, List<Type>> bindings)
        {
            if (inst.IsGenericType == false) return false;

            var instd = inst.GetGenericTypeDefinition();
            var typed = type.GetGenericTypeDefinition();

            if (instd != typed) return false;

            var insta = inst.GetGenericArguments();
            var typea = type.GetGenericArguments();

            if (insta.Length != typea.Length) return false;

            // but... there is no ZipWhile, and we have arrays anyway
            //var x = insta.Zip<Type, Type, bool>(typea, (instax, typeax) => { ... });

            for (var i = 0; i < insta.Length; i++)
                if (MatchType(insta[i], typea[i], bindings) == false)
                    return false;

            return true;
        }

        private static IEnumerable<Type> GetGenericParameterCandidateTypes(Type type)
        {
            yield return type;
            var t = type.BaseType;
            while (t != null)
            {
                yield return t;
                t = t.BaseType;
            }
            foreach (var i in type.GetInterfaces())
                yield return i;
        }

        public static bool MatchType(Type inst, Type type)
        {
            return MatchType(inst, type, new Dictionary<string, List<Type>>());
        }

        internal static bool MatchType(Type inst, Type type, IDictionary<string, List<Type>> bindings)
        {
            if (type.IsGenericType)
            {
                if (MatchGeneric(inst, type, bindings)) return true;
                var t = inst.BaseType;
                while (t != null)
                {
                    if (MatchGeneric(t, type, bindings)) return true;
                    t = t.BaseType;
                }
                return inst.GetInterfaces().Any(i => MatchGeneric(i, type, bindings));
            }

            if (type.IsGenericParameter)
            {
                if (bindings.ContainsKey(type.Name))
                {
                    ReduceGenericParameterCandidateTypes(bindings[type.Name], inst);
                    return bindings[type.Name].Count > 0;
                }

                bindings[type.Name] = new List<Type>(GetGenericParameterCandidateTypes(inst));
                return true;
            }

            if (inst == type) return true;
            if (type.IsClass && inst.IsClass && inst.IsSubclassOf(type)) return true;
            if (type.IsInterface && inst.GetInterfaces().Contains(type)) return true;

            return false;
        }

        #endregion
	}
}