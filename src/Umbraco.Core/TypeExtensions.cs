using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Umbraco.Core
{
    public static class TypeExtensions
    {
        internal static MethodInfo GetGenericMethod(this Type type, string name, params Type[] parameterTypes)
        {
            var methods = type.GetMethods().Where(method => method.Name == name);

            foreach (var method in methods)
            {
                if (method.HasParameters(parameterTypes))
                    return method;
            }

            return null;
        }

        internal static bool HasParameters(this MethodInfo method, params Type[] parameterTypes)
        {
            var methodParameters = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();

            if (methodParameters.Length != parameterTypes.Length)
                return false;

            for (int i = 0; i < methodParameters.Length; i++)
                if (methodParameters[i].ToString() != parameterTypes[i].ToString())
                    return false;

            return true;
        }

        public static IEnumerable<Type> AllInterfaces(this Type target)
        {
            foreach (var IF in target.GetInterfaces())
            {
                yield return IF;
                foreach (var childIF in IF.AllInterfaces())
                {
                    yield return childIF;
                }
            }
        }

        public static IEnumerable<MethodInfo> AllMethods(this Type target)
        {
            var allTypes = target.AllInterfaces().ToList();
            allTypes.Add(target);

            return from type in allTypes
                   from method in type.GetMethods()
                   select method;
        }
    }
}