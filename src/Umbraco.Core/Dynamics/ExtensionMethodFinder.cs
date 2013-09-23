using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Linq.Expressions;

namespace Umbraco.Core.Dynamics
{
	/// <summary>
	/// Utility class for finding extension methods on a type to execute
	/// </summary>
    internal static class ExtensionMethodFinder
	{
	    private static readonly MethodInfo[] AllExtensionMethods;

	    static ExtensionMethodFinder()
	    {
            AllExtensionMethods = TypeFinder.GetAssembliesWithKnownExclusions()
                // assemblies that contain extension methods
                .Where(a => a.IsDefined(typeof(ExtensionAttribute), false))
                // types that contain extension methods
                .SelectMany(a => a.GetTypes()
                    .Where(t => t.IsDefined(typeof(ExtensionAttribute), false) && t.IsSealed && t.IsGenericType == false && t.IsNested == false))
                // actual extension methods
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(m => m.IsDefined(typeof(ExtensionAttribute), false)))
                // and also IEnumerable<T> extension methods - because the assembly is excluded
                .Concat(typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public))
                .ToArray();
	    }

        // ORIGINAL CODE IS NOT COMPLETE, DOES NOT HANDLE GENERICS, ETC...

        // so this is an attempt at fixing things, but it's not done yet
        // and do we really want to do this? extension methods are not supported on dynamics, period
        // we should use strongly typed content instead of dynamics.

        /*

        // get all extension methods for type thisType, with name name,
        // accepting argsCount arguments (not counting the instance of thisType).
        private static IEnumerable<MethodInfo> GetExtensionMethods(Type thisType, string name, int argsCount)
        {
            var key = string.Format("{0}.{1}::{2}", thisType.FullName, name, argsCount);

            var types = thisType.GetBaseTypes(true); // either do this OR have MatchFirstParameter handle the stuff... F*XME

            var methods = AllExtensionMethods
                .Where(m => m.Name == name)
                .Where(m => m.GetParameters().Length == argsCount)
                .Where(m => MatchFirstParameter(thisType, m.GetParameters()[0].ParameterType));

            // f*xme - is this what we should cache?
            return methods;
        }

        // find out whether the first parameter is a match for thisType
        private static bool MatchFirstParameter(Type thisType, Type firstParameterType)
        {
            return MethodArgZeroHasCorrectTargetType(null, firstParameterType, thisType);
        }

        // get the single extension method for type thisType, with name name,
        // that accepts the arguments in args (which does not contain the instance of thisType).
        public static MethodInfo GetExtensionMethod(Type thisType, string name, object[] args)
        {
            MethodInfo result = null;
            foreach (var method in GetExtensionMethods(thisType, name, args.Length).Where(m => MatchParameters(m, args)))
            {
                if (result == null)
                    result = method;
                else
                    throw new AmbiguousMatchException("More than one matching extension method was found.");
            }
            return result;
        }

        // find out whether the method can accept the arguments
        private static bool MatchParameters(MethodInfo method, IList<object> args)
        {
            var parameters = method.GetParameters();

            var i = 0;
            for (; i < parameters.Length; ++i)
            {
                if (MatchParameter(parameters[i].ParameterType, args[i].GetType()) == false)
                    break;
            }
            return (i == parameters.Length);
        }

        internal static bool MatchParameter(Type parameterType, Type argumentType)
        {
            // public static int DoSomething<T>(Foo foo, T t1, T t2)
            // DoSomething(foo, t1, t2) => how can we match?!
            return parameterType == argumentType; // f*xme of course!
        }
         * 
        */

        // BELOW IS THE ORIGINAL CODE...

		/// <summary>
		/// Returns all extension methods found matching the definition
		/// </summary>
		/// <param name="thisType"></param>
		/// <param name="name"></param>
		/// <param name="argumentCount"></param>
		/// <param name="argsContainsThis"></param>
		/// <returns></returns>
		/// <remarks>
		/// TODO: NOTE: This will be an intensive method to call!! Results should be cached!
		/// </remarks>
        private static IEnumerable<MethodInfo> GetAllExtensionMethods(Type thisType, string name, int argumentCount, bool argsContainsThis)
        {
            // at *least* we can cache the extension methods discovery
		    var candidates = AllExtensionMethods;

            /*
			//only scan assemblies we know to contain extension methods (user assemblies)
        	var assembliesToScan = TypeFinder.GetAssembliesWithKnownExclusions();        	

            //get extension methods from runtime
            var candidates = (
				from assembly in assembliesToScan
                where assembly.IsDefined(typeof(ExtensionAttribute), false)
                from type in assembly.GetTypes()
                where (type.IsDefined(typeof(ExtensionAttribute), false)
                    && type.IsSealed && !type.IsGenericType && !type.IsNested)
                from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                // this filters extension methods
                where method.IsDefined(typeof(ExtensionAttribute), false)
                select method
                );

            //add the extension methods defined in IEnumerable
            candidates = candidates.Concat(typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public));            
            */

            //filter by name	
            var methodsByName = candidates.Where(m => m.Name == name);

            var isGenericAndRightParamCount = methodsByName.Where(m => m.GetParameters().Length == argumentCount + (argsContainsThis ? 0 : 1));

            //find the right overload that can take genericParameterType
            //which will be either DynamicNodeList or List<DynamicNode> which is IEnumerable`

            var withGenericParameterType = isGenericAndRightParamCount.Select(m => new { m, t = FirstParameterType(m) });

            var methodsWhereArgZeroIsTargetType = (from method in withGenericParameterType
                                                   where
                                                   method.t != null && MethodArgZeroHasCorrectTargetType(method.m, method.t, thisType)
                                                   select method);

            return methodsWhereArgZeroIsTargetType.Select(mt => mt.m);
        }
        
		private static bool MethodArgZeroHasCorrectTargetType(MethodInfo method, Type firstArgumentType, Type thisType)
        {
            //This is done with seperate method calls because you can't debug/watch lamdas - if you're trying to figure
            //out why the wrong method is returned, it helps to be able to see each boolean result

            return

            // is it defined on me?
            MethodArgZeroHasCorrectTargetTypeTypeMatchesExactly(method, firstArgumentType, thisType) ||

            // or on any of my interfaces?
           MethodArgZeroHasCorrectTargetTypeAnInterfaceMatches(method, firstArgumentType, thisType) ||

            // or on any of my base types?
            MethodArgZeroHasCorrectTargetTypeIsASubclassOf(method, firstArgumentType, thisType) ||

           //share a common interface (e.g. IEnumerable)
            MethodArgZeroHasCorrectTargetTypeShareACommonInterface(method, firstArgumentType, thisType);


        }

        private static bool MethodArgZeroHasCorrectTargetTypeShareACommonInterface(MethodInfo method, Type firstArgumentType, Type thisType)
        {
            Type[] interfaces = firstArgumentType.GetInterfaces();
            if (interfaces.Length == 0)
            {
                return false;
            }
            bool result = interfaces.All(i => thisType.GetInterfaces().Contains(i));
            return result;
        }

        private static bool MethodArgZeroHasCorrectTargetTypeIsASubclassOf(MethodInfo method, Type firstArgumentType, Type thisType)
        {
            bool result = thisType.IsSubclassOf(firstArgumentType);
            return result;
        }

        private static bool MethodArgZeroHasCorrectTargetTypeAnInterfaceMatches(MethodInfo method, Type firstArgumentType, Type thisType)
        {
            bool result = thisType.GetInterfaces().Contains(firstArgumentType);
            return result;
        }

        private static bool MethodArgZeroHasCorrectTargetTypeTypeMatchesExactly(MethodInfo method, Type firstArgumentType, Type thisType)
        {
            bool result = (thisType == firstArgumentType);
            return result;
        }
        
		private static Type FirstParameterType(MethodInfo m)
        {
            ParameterInfo[] p = m.GetParameters();
            if (p.Any())
            {
                return p.First().ParameterType;
            }
            return null;
        }

		private static MethodInfo DetermineMethodFromParams(IEnumerable<MethodInfo> methods, Type genericType, IEnumerable<object> args)
		{
			if (!methods.Any())
			{
				return null;
			}
			MethodInfo methodToExecute = null;
			if (methods.Count() > 1)
			{
				//Given the args, lets get the types and compare the type sequence to try and find the correct overload
				var argTypes = args.ToList().ConvertAll(o =>
				{
					var oe = (o as Expression);
					if (oe != null)
					{
						return oe.Type.FullName;
					}
					return o.GetType().FullName;
				});
				var methodsWithArgTypes = methods.Select(method => new { method, types = method.GetParameters().Select(pi => pi.ParameterType.FullName) });
				var firstMatchingOverload = methodsWithArgTypes.FirstOrDefault(m => m.types.SequenceEqual(argTypes));
				if (firstMatchingOverload != null)
				{
					methodToExecute = firstMatchingOverload.method;
				}
			}

			if (methodToExecute == null)
			{
				var firstMethod = methods.FirstOrDefault();
				// NH: this is to ensure that it's always the correct one being chosen when using the LINQ extension methods
				if (methods.Count() > 1)
				{
					var firstGenericMethod = methods.FirstOrDefault(x => x.IsGenericMethodDefinition);
					if (firstGenericMethod != null)
					{
						firstMethod = firstGenericMethod;
					}
				}

				if (firstMethod != null)
				{
					if (firstMethod.IsGenericMethodDefinition)
					{
						if (genericType != null)
						{
							methodToExecute = firstMethod.MakeGenericMethod(genericType);
						}
					}
					else
					{
						methodToExecute = firstMethod;
					}
				}
			}
			return methodToExecute;
		}

        public static MethodInfo FindExtensionMethod(Type thisType, object[] args, string name, bool argsContainsThis)
        {
            Type genericType = null;
            if (thisType.IsGenericType)
            {
                genericType = thisType.GetGenericArguments()[0];
            }			

        	var methods = GetAllExtensionMethods(thisType, name, args.Length, argsContainsThis).ToArray();
			return DetermineMethodFromParams(methods, genericType, args);
        }
    }
}
