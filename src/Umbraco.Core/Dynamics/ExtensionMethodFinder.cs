using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using Umbraco.Core.Cache;

namespace Umbraco.Core.Dynamics
{
	/// <summary>
	/// Utility class for finding extension methods on a type to execute
	/// </summary>
    internal static class ExtensionMethodFinder
	{
        /// <summary>
        /// The static cache for extension methods found that match the criteria that we are looking for
        /// </summary>
        private static readonly ConcurrentDictionary<Tuple<Type, string, int>, MethodInfo[]> MethodCache = new ConcurrentDictionary<Tuple<Type, string, int>, MethodInfo[]>();

        /// <summary>
        /// Returns the enumerable of all extension method info's in the app domain = USE SPARINGLY!!!
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// We cache this as a sliding 5 minute exiration, in unit tests there's over 1100 methods found, surely that will eat up a bit of memory so we want
        /// to make sure we give it back.
        /// </remarks>
	    private static IEnumerable<MethodInfo> GetAllExtensionMethodsInAppDomain(IRuntimeCacheProvider runtimeCacheProvider)
        {
            if (runtimeCacheProvider == null) throw new ArgumentNullException("runtimeCacheProvider");

            return runtimeCacheProvider.GetCacheItem<MethodInfo[]>(typeof (ExtensionMethodFinder).Name, () => TypeFinder.GetAssembliesWithKnownExclusions()
                // assemblies that contain extension methods
                .Where(a => a.IsDefined(typeof (ExtensionAttribute), false))
                // types that contain extension methods
                .SelectMany(a => a.GetTypes()
                    .Where(t => t.IsDefined(typeof (ExtensionAttribute), false) && t.IsSealed && t.IsGenericType == false && t.IsNested == false))
                // actual extension methods
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(m => m.IsDefined(typeof (ExtensionAttribute), false)))
                // and also IEnumerable<T> extension methods - because the assembly is excluded
                .Concat(typeof (Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public))
                //If we don't do this then we'll be scanning all assemblies each time!
                .ToArray(),
                
                //only cache for 5 minutes
                timeout: TimeSpan.FromMinutes(5), 

                //each time this is accessed it will be for 5 minutes longer
                isSliding:true);
        }

	    /// <summary>
	    /// Returns all extension methods found matching the definition
	    /// </summary>
	    /// <param name="runtimeCache">
	    /// The runtime cache is used to temporarily cache all extension methods found in the app domain so that 
	    /// while we search for individual extension methods, the process will be reasonably 'quick'. We then statically
	    /// cache the MethodInfo's that we are looking for and then the runtime cache will expire and give back all that memory.
	    /// </param>
	    /// <param name="thisType"></param>
	    /// <param name="name"></param>
	    /// <param name="argumentCount">
	    /// The arguments EXCLUDING the 'this' argument in an extension method
	    /// </param>
	    /// <returns></returns>
	    /// <remarks>
	    /// NOTE: This will be an intensive method to call! Results will be cached based on the key (args) of this method
	    /// </remarks>
	    internal static IEnumerable<MethodInfo> GetAllExtensionMethods(IRuntimeCacheProvider runtimeCache, Type thisType, string name, int argumentCount)
		{
		    var key = new Tuple<Type, string, int>(thisType, name, argumentCount);

		    return MethodCache.GetOrAdd(key, tuple =>
		    {
                var candidates = GetAllExtensionMethodsInAppDomain(runtimeCache);


                //filter by name	
                var methodsByName = candidates.Where(m => m.Name == name);

                //ensure we add + 1 to the arg count because the 'this' arg is not included in the count above!
                var byParamCount = methodsByName.Where(m => m.GetParameters().Length == argumentCount + 1);

		        var methodsWithFirstParamType = byParamCount.Select(m => new {m, t = FirstParameterType(m)})
		            .ToArray(); // get the array so we don't keep enumerating.

                //Find the method with an assignable first parameter type
		        var methodsWhereArgZeroIsTargetType = (from method in methodsWithFirstParamType
		            where
		                method.t != null && TypeHelper.IsTypeAssignableFrom(method.t, thisType)
		            select method).ToArray();
                
                //found some so return this
		        if (methodsWhereArgZeroIsTargetType.Any()) return methodsWhereArgZeroIsTargetType.Select(mt => mt.m).ToArray();

                //this is where it gets tricky, none of the first parameters were assignable to our type which means that
                // if they are assignable they are generic arguments

		        methodsWhereArgZeroIsTargetType = (from method in methodsWithFirstParamType
		            where
		                method.t != null && TypeHelper.IsAssignableToGenericType(method.t, thisType)
		            select method).ToArray();

                return methodsWhereArgZeroIsTargetType.Select(mt => mt.m).ToArray();
		    });
		    
        }
    
        private static Type FirstParameterType(MethodInfo m)
        {
            var p = m.GetParameters();
            if (p.Any())
            {
                return p.First().ParameterType;
            }
            return null;
        }

        private static MethodInfo DetermineMethodFromParams(IEnumerable<MethodInfo> methods, Type thisType, IEnumerable<object> args)
		{
			MethodInfo methodToExecute = null;

            //Given the args, lets get the types and compare the type sequence to try and find the correct overload
            var argTypes = args.Select(o =>
            {
                var oe = (o as Expression);
                return oe != null ? oe.Type : o.GetType();
            });

            var methodsWithArgTypes = methods.Select(method => new
            {
                method,
                //skip the first arg because that is the extension method type ('this') that we are looking for
                types = method.GetParameters().Select(pi => pi.ParameterType).Skip(1)
            });

            //This type comparer will check 
            var typeComparer = new DelegateEqualityComparer<Type>(
                //Checks if the argument type passed in can be assigned from the parameter type in the method. For 
                // example, if the argument type is HtmlHelper<MyModel> but the method parameter type is HtmlHelper then
                // it will match because the argument is assignable to that parameter type and will be able to execute
                TypeHelper.IsTypeAssignableFrom, 
                //This will not ever execute but if it does we need to get the hash code of the string because the hash
                // code of a type is random
                type => type.FullName.GetHashCode());

            var firstMatchingOverload = methodsWithArgTypes
                .FirstOrDefault(m => m.types.SequenceEqual(argTypes, typeComparer));

            if (firstMatchingOverload != null)
            {
                methodToExecute = firstMatchingOverload.method;

                var extensionParam = methodToExecute.GetParameters()[0].ParameterType;

                //We've already done this check before in the GetAllExtensionMethods method, but need to do it here
                // again because in order to use this found generic method we need to create a real generic method
                // with the generic type parameters
                var baseCompareTypeAttempt = TypeHelper.IsAssignableToGenericType(extensionParam, thisType);
                //This should never throw
                if (baseCompareTypeAttempt.Success == false) throw new InvalidOperationException("No base compare type could be resolved");

                if (methodToExecute.IsGenericMethodDefinition && baseCompareTypeAttempt.Result.IsGenericType)
                {
                    methodToExecute = methodToExecute.MakeGenericMethod(baseCompareTypeAttempt.Result.GetGenericArguments());
                }
            }

            return methodToExecute;
		}

        public static MethodInfo FindExtensionMethod(IRuntimeCacheProvider runtimeCache, Type thisType, object[] args, string name, bool argsContainsThis)
        {
            args = args
                //if the args contains 'this', remove the first one since that is 'this' and we don't want to use
                //that in the method searching
                .Skip(argsContainsThis ? 1 : 0)
                .ToArray();

            var methods = GetAllExtensionMethods(runtimeCache, thisType, name, args.Length).ToArray();

            return DetermineMethodFromParams(methods, thisType, args);
        }
    }
}
