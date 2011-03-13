using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Compilation;
using System.Runtime.CompilerServices;
using System.Collections;

namespace umbraco.MacroEngines
{
    public static class ExtensionMethodFinder
    {
        private static List<MethodInfo> GetAllExtensionMethods(Type thisType, string name, int argumentCount, bool argsContainsThis)
        {
            //get extension methods from runtime
            var candidates = (
                from assembly in BuildManager.GetReferencedAssemblies().Cast<Assembly>()
                where assembly.IsDefined(typeof(ExtensionAttribute), false)
                from type in assembly.GetTypes()
                where (type.IsDefined(typeof(ExtensionAttribute), false)
                    && type.IsSealed && !type.IsGenericType && !type.IsNested)
                from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                // this filters extension methods
                where method.IsDefined(typeof(ExtensionAttribute), false)
                select method
                );

            //search an explicit type (e.g. Enumerable, where most of the Linq methods are defined)
            //if (explicitTypeToSearch != null)
            //{
            candidates = candidates.Concat(typeof(IEnumerable).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
            //}

            //filter by name
            var methodsByName = candidates.Where(m => m.Name == name);

            var isGenericAndRightParamCount = methodsByName.Where(m => m.GetParameters().Length == argumentCount + (argsContainsThis ? 0 : 1));

            //find the right overload that can take genericParameterType
            //which will be either DynamicNodeList or List<DynamicNode> which is IEnumerable`

            var withGenericParameterType = isGenericAndRightParamCount.Select(m => new { m, t = firstParameterType(m) });

            var methodsWhereArgZeroIsTargetType = (from method in withGenericParameterType
                                                   where
                                                   method.t != null && methodArgZeroHasCorrectTargetType(method.m, method.t, thisType)
                                                   select method);

            return methodsWhereArgZeroIsTargetType.Select(mt => mt.m).ToList();
        }
        private static bool methodArgZeroHasCorrectTargetType(MethodInfo method, Type firstArgumentType, Type thisType)
        {
            //This is done with seperate method calls because you can't debug/watch lamdas - if you're trying to figure
            //out why the wrong method is returned, it helps to be able to see each boolean result

            return

            // is it defined on me?
            methodArgZeroHasCorrectTargetType_TypeMatchesExactly(method, firstArgumentType, thisType) ||

            // or on any of my interfaces?
           methodArgZeroHasCorrectTargetType_AnInterfaceMatches(method, firstArgumentType, thisType) ||

            // or on any of my base types?
            methodArgZeroHasCorrectTargetType_IsASubclassOf(method, firstArgumentType, thisType) ||

           //share a common interface (e.g. IEnumerable)
            methodArgZeroHasCorrectTargetType_ShareACommonInterface(method, firstArgumentType, thisType);


        }

        private static bool methodArgZeroHasCorrectTargetType_ShareACommonInterface(MethodInfo method, Type firstArgumentType, Type thisType)
        {
            Type[] interfaces = firstArgumentType.GetInterfaces();
            if (interfaces.Length == 0)
            {
                return false;
            }
            bool result = interfaces.All(i => thisType.GetInterfaces().Contains(i));
            return result;
        }

        private static bool methodArgZeroHasCorrectTargetType_IsASubclassOf(MethodInfo method, Type firstArgumentType, Type thisType)
        {
            bool result = thisType.IsSubclassOf(firstArgumentType);
            return result;
        }

        private static bool methodArgZeroHasCorrectTargetType_AnInterfaceMatches(MethodInfo method, Type firstArgumentType, Type thisType)
        {
            bool result = thisType.GetInterfaces().Contains(firstArgumentType);
            return result;
        }

        private static bool methodArgZeroHasCorrectTargetType_TypeMatchesExactly(MethodInfo method, Type firstArgumentType, Type thisType)
        {
            bool result = (thisType == firstArgumentType);
            return result;
        }
        private static Type firstParameterType(MethodInfo m)
        {
            ParameterInfo[] p = m.GetParameters();
            if (p.Count() > 0)
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

            MethodInfo firstMethod = methods.First();
            // NH: this is to ensure that it's always the correct one being chosen when using the LINQ extension methods
            if (methods.Count > 1)
                firstMethod = methods.First(x => x.IsGenericMethodDefinition);

            MethodInfo methodToExecute = null;
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
            return methodToExecute;
        }
    }
}
