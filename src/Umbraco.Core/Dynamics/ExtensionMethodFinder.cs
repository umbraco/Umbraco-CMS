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
    internal static class ExtensionMethodFinder
    {
		


        private static List<MethodInfo> GetAllExtensionMethods(Type thisType, string name, int argumentCount, bool argsContainsThis)
        {
			//only scan assemblies we know to contain extension methods (user assemblies)
        	//var assembliesToScan = TypeFinder.GetAssembliesWithKnownExclusions();
        	var assembliesToScan = new List<Assembly>()
        		{
        			Assembly.Load("Umbraco.Tests")
        		};

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

            //search an explicit type (e.g. Enumerable, where most of the Linq methods are defined)
            //if (explicitTypeToSearch != null)
            //{
            candidates = candidates.Concat(typeof(IEnumerable).GetMethods(BindingFlags.Static | BindingFlags.Public));
            //}

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

            return methodsWhereArgZeroIsTargetType.Select(mt => mt.m).ToList();
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

        public static MethodInfo FindExtensionMethod(Type thisType, object[] args, string name, bool argsContainsThis)
        {
            Type genericType = null;
            if (thisType.IsGenericType)
            {
                genericType = thisType.GetGenericArguments()[0];
            }

            var methods = GetAllExtensionMethods(thisType, name, args.Length, argsContainsThis);

            if (methods.Count == 0)
            {
                return null;
            }
            MethodInfo methodToExecute = null;
            if (methods.Count > 1)
            {
                //Given the args, lets get the types and compare the type sequence to try and find the correct overload
                var argTypes = args.ToList().ConvertAll(o =>
                {
                    Expression oe = (o as Expression);
                    if (oe != null)
                    {
                        return oe.Type.FullName;
                    }
                    return o.GetType().FullName;
                });
                var methodsWithArgTypes = methods.ConvertAll(method => new { method = method, types = method.GetParameters().ToList().ConvertAll(pi => pi.ParameterType.FullName) });
                var firstMatchingOverload = methodsWithArgTypes.FirstOrDefault(m =>
                {
                    return m.types.SequenceEqual(argTypes);
                });
                if (firstMatchingOverload != null)
                {
                    methodToExecute = firstMatchingOverload.method;
                }
            }

            if (methodToExecute == null)
            {
                MethodInfo firstMethod = methods.FirstOrDefault();
                // NH: this is to ensure that it's always the correct one being chosen when using the LINQ extension methods
                if (methods.Count > 1)
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
    }
}
