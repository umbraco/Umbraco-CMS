// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Extensions;

public static class TypeExtensions
{
    public static object? GetDefaultValue(this Type t) =>
        t.IsValueType
            ? Activator.CreateInstance(t)
            : null;

    /// <summary>
    ///     Checks if the type is an anonymous type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <remarks>
    ///     reference: http://jclaes.blogspot.com/2011/05/checking-for-anonymous-types.html
    /// </remarks>
    public static bool IsAnonymousType(this Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException("type");
        }

        return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
               && type.IsGenericType && type.Name.Contains("AnonymousType")
               && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
               && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
    }

    public static IEnumerable<Type?> GetBaseTypes(this Type? type, bool andSelf)
    {
        if (andSelf)
        {
            yield return type;
        }

        while ((type = type?.BaseType) != null)
        {
            yield return type;
        }
    }

    internal static MethodInfo? GetGenericMethod(this Type type, string name, params Type[] parameterTypes)
    {
        IEnumerable<MethodInfo> methods = type.GetMethods().Where(method => method.Name == name);

        foreach (MethodInfo method in methods)
        {
            if (method.HasParameters(parameterTypes))
            {
                return method;
            }
        }

        return null;
    }

    /// <summary>
    ///     Determines whether the specified type is enumerable.
    /// </summary>
    /// <param name="method">The type.</param>
    /// <param name="parameterTypes"></param>
    internal static bool HasParameters(this MethodInfo method, params Type[] parameterTypes)
    {
        Type[] methodParameters = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();

        if (methodParameters.Length != parameterTypes.Length)
        {
            return false;
        }

        for (var i = 0; i < methodParameters.Length; i++)
        {
            if (methodParameters[i].ToString() != parameterTypes[i].ToString())
            {
                return false;
            }
        }

        return true;
    }

    public static IEnumerable<MethodInfo> AllMethods(this Type target)
    {
        // var allTypes = target.AllInterfaces().ToList();
        var allTypes = target.GetInterfaces().ToList(); // GetInterfaces is ok here
        allTypes.Add(target);

        return allTypes.SelectMany(t => t.GetMethods());
    }

    /// <returns>
    ///     <c>true</c> if the specified type is enumerable; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsEnumerable(this Type type)
    {
        if (type.IsGenericType)
        {
            if (type.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable)))
            {
                return true;
            }
        }
        else
        {
            if (type.GetInterfaces().Contains(typeof(IEnumerable)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Determines whether [is of generic type] [the specified type].
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="genericType">Type of the generic.</param>
    /// <returns>
    ///     <c>true</c> if [is of generic type] [the specified type]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsOfGenericType(this Type type, Type genericType)
    {
        return type.TryGetGenericArguments(genericType, out Type[]? args);
    }

    /// <summary>
    ///     Will find the generic type of the 'type' parameter passed in that is equal to the 'genericType' parameter passed in
    /// </summary>
    /// <param name="type"></param>
    /// <param name="genericType"></param>
    /// <param name="genericArgType"></param>
    /// <returns></returns>
    public static bool TryGetGenericArguments(this Type type, Type genericType, out Type[]? genericArgType)
    {
        if (type == null)
        {
            throw new ArgumentNullException("type");
        }

        if (genericType == null)
        {
            throw new ArgumentNullException("genericType");
        }

        if (genericType.IsGenericType == false)
        {
            throw new ArgumentException("genericType must be a generic type");
        }

        Func<Type, Type, Type[]?> checkGenericType = (@int, t) =>
        {
            if (@int.IsGenericType)
            {
                Type def = @int.GetGenericTypeDefinition();
                if (def == t)
                {
                    return @int.GetGenericArguments();
                }
            }

            return null;
        };

        // first, check if the type passed in is already the generic type
        genericArgType = checkGenericType(type, genericType);
        if (genericArgType != null)
        {
            return true;
        }

        // if we're looking for interfaces, enumerate them:
        if (genericType.IsInterface)
        {
            foreach (Type @interface in type.GetInterfaces())
            {
                genericArgType = checkGenericType(@interface, genericType);
                if (genericArgType != null)
                {
                    return true;
                }
            }
        }
        else
        {
            // loop back into the base types as long as they are generic
            while (type.BaseType != null && type.BaseType != typeof(object))
            {
                genericArgType = checkGenericType(type.BaseType, genericType);
                if (genericArgType != null)
                {
                    return true;
                }

                type = type.BaseType;
            }
        }

        return false;
    }

    /// <summary>
    ///     Gets all properties in a flat hierarchy
    /// </summary>
    /// <remarks>Includes both Public and Non-Public properties</remarks>
    /// <param name="type"></param>
    /// <returns></returns>
    public static PropertyInfo[] GetAllProperties(this Type type)
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
                Type subType = queue.Dequeue();
                foreach (Type subInterface in subType.GetInterfaces())
                {
                    if (considered.Contains(subInterface))
                    {
                        continue;
                    }

                    considered.Add(subInterface);
                    queue.Enqueue(subInterface);
                }

                PropertyInfo[] typeProperties = subType.GetProperties(
                    BindingFlags.FlattenHierarchy
                    | BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance);

                IEnumerable<PropertyInfo> newPropertyInfos = typeProperties
                    .Where(x => !propertyInfos.Contains(x));

                propertyInfos.InsertRange(0, newPropertyInfos);
            }

            return propertyInfos.ToArray();
        }

        return type.GetProperties(BindingFlags.FlattenHierarchy
                                  | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    /// <summary>
    ///     Returns all public properties including inherited properties even for interfaces
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <remarks>
    ///     taken from
    ///     http://stackoverflow.com/questions/358835/getproperties-to-return-all-properties-for-an-interface-inheritance-hierarchy
    /// </remarks>
    public static PropertyInfo[] GetPublicProperties(this Type type)
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
                Type subType = queue.Dequeue();
                foreach (Type subInterface in subType.GetInterfaces())
                {
                    if (considered.Contains(subInterface))
                    {
                        continue;
                    }

                    considered.Add(subInterface);
                    queue.Enqueue(subInterface);
                }

                PropertyInfo[] typeProperties = subType.GetProperties(
                    BindingFlags.FlattenHierarchy
                    | BindingFlags.Public
                    | BindingFlags.Instance);

                IEnumerable<PropertyInfo> newPropertyInfos = typeProperties
                    .Where(x => !propertyInfos.Contains(x));

                propertyInfos.InsertRange(0, newPropertyInfos);
            }

            return propertyInfos.ToArray();
        }

        return type.GetProperties(BindingFlags.FlattenHierarchy
                                  | BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>
    ///     Determines whether the specified actual type is type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="actualType">The actual type.</param>
    /// <returns>
    ///     <c>true</c> if the specified actual type is type; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsType<T>(this Type actualType) => TypeHelper.IsTypeAssignableFrom<T>(actualType);

    public static bool Inherits<TBase>(this Type type) => typeof(TBase).IsAssignableFrom(type);

    public static bool Inherits(this Type type, Type tbase) => tbase.IsAssignableFrom(type);

    public static bool Implements<TInterface>(this Type type) => typeof(TInterface).IsAssignableFrom(type);

    public static TAttribute? FirstAttribute<TAttribute>(this Type type) => type.FirstAttribute<TAttribute>(true);

    public static TAttribute? FirstAttribute<TAttribute>(this Type type, bool inherit)
    {
        var attrs = type.GetCustomAttributes(typeof(TAttribute), inherit);
        return (TAttribute?)(attrs.Length > 0 ? attrs[0] : null);
    }

    public static TAttribute? FirstAttribute<TAttribute>(this PropertyInfo propertyInfo) =>
        propertyInfo.FirstAttribute<TAttribute>(true);

    public static TAttribute? FirstAttribute<TAttribute>(this PropertyInfo propertyInfo, bool inherit)
    {
        var attrs = propertyInfo.GetCustomAttributes(typeof(TAttribute), inherit);
        return (TAttribute?)(attrs.Length > 0 ? attrs[0] : null);
    }

    public static IEnumerable<TAttribute>? MultipleAttribute<TAttribute>(this PropertyInfo propertyInfo) =>
        propertyInfo.MultipleAttribute<TAttribute>(true);

    public static IEnumerable<TAttribute>? MultipleAttribute<TAttribute>(this PropertyInfo propertyInfo, bool inherit)
    {
        var attrs = propertyInfo.GetCustomAttributes(typeof(TAttribute), inherit);
        return attrs.Length > 0 ? attrs.ToList().ConvertAll(input => (TAttribute)input) : null;
    }

    /// <summary>
    ///     Returns the full type name with the assembly but without all of the assembly specific version information.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This method is like an 'in between' of Type.FullName and Type.AssemblyQualifiedName which returns the type and the
    ///     assembly separated
    ///     by a comma.
    /// </remarks>
    /// <example>
    ///     The output of this class would be:
    ///     Umbraco.Core.TypeExtensions, Umbraco.Core
    /// </example>
    public static string GetFullNameWithAssembly(this Type type)
    {
        AssemblyName assemblyName = type.Assembly.GetName();

        return string.Concat(type.FullName, ", ", assemblyName.FullName.StartsWith("App_Code.") ? "App_Code" : assemblyName.Name);
    }

    /// <summary>
    ///     Determines whether an instance of a specified type can be assigned to the current type instance.
    /// </summary>
    /// <param name="type">The current type.</param>
    /// <param name="c">The type to compare with the current type.</param>
    /// <returns>A value indicating whether an instance of the specified type can be assigned to the current type instance.</returns>
    /// <remarks>
    ///     This extended version supports the current type being a generic type definition, and will
    ///     consider that eg <c>List{int}</c> is "assignable to" <c>IList{}</c>.
    /// </remarks>
    public static bool IsAssignableFromGtd(this Type type, Type c)
    {
        // type *can* be a generic type definition
        // c is a real type, cannot be a generic type definition
        if (type.IsGenericTypeDefinition == false)
        {
            return type.IsAssignableFrom(c);
        }

        if (c.IsInterface == false)
        {
            Type? t = c;
            while (t != typeof(object))
            {
                if (t is not null && t.IsGenericType && t.GetGenericTypeDefinition() == type)
                {
                    return true;
                }

                t = t?.BaseType;
            }
        }

        return c.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == type);
    }

    /// <summary>
    ///     If the given <paramref name="type" /> is an array or some other collection
    ///     comprised of 0 or more instances of a "subtype", get that type
    /// </summary>
    /// <param name="type">the source type</param>
    /// <returns></returns>
    public static Type? GetEnumeratedType(this Type type)
    {
        if (typeof(IEnumerable).IsAssignableFrom(type) == false)
        {
            return null;
        }

        // provided by Array
        Type? elType = type.GetElementType();
        if (elType != null)
        {
            return elType;
        }

        // otherwise provided by collection
        Type[] elTypes = type.GetGenericArguments();
        if (elTypes.Length > 0)
        {
            return elTypes[0];
        }

        // otherwise is not an 'enumerated' type
        return null;
    }

    public static T? GetCustomAttribute<T>(this Type type, bool inherit)
        where T : Attribute =>
        type.GetCustomAttributes<T>(inherit).SingleOrDefault();

    public static IEnumerable<T> GetCustomAttributes<T>(this Type? type, bool inherited)
        where T : Attribute
    {
        if (type == null)
        {
            return Enumerable.Empty<T>();
        }

        return type.GetCustomAttributes(typeof(T), inherited).OfType<T>();
    }

    public static bool HasCustomAttribute<T>(this Type type, bool inherit)
        where T : Attribute =>
        type.GetCustomAttribute<T>(inherit) != null;

    /// <summary>
    ///     Tries to return a value based on a property name for an object but ignores case sensitivity
    /// </summary>
    /// <param name="type"></param>
    /// <param name="shortStringHelper"></param>
    /// <param name="target"></param>
    /// <param name="memberName"></param>
    /// <returns></returns>
    /// <remarks>
    ///     Currently this will only work for ProperCase and camelCase properties, see the TODO below to enable complete case
    ///     insensitivity
    /// </remarks>
    internal static Attempt<object> GetMemberIgnoreCase(this Type type, IShortStringHelper shortStringHelper, object target, string memberName)
    {
        Func<string, Attempt<object>> getMember =
            memberAlias =>
            {
                try
                {
                    return Attempt<object>.Succeed(
                        type.InvokeMember(
                            memberAlias,
                            BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public,
                            null,
                            target,
                            null));
                }
                catch (MissingMethodException ex)
                {
                    return Attempt<object>.Fail(ex);
                }
            };

        // try with the current casing
        Attempt<object> attempt = getMember(memberName);
        if (attempt.Success == false)
        {
            // if we cannot get with the current alias, try changing it's case
            attempt = memberName[0].IsUpperCase()
                ? getMember(memberName.ToCleanString(
                    shortStringHelper,
                    CleanStringType.Ascii | CleanStringType.ConvertCase | CleanStringType.CamelCase))
                : getMember(memberName.ToCleanString(
                    shortStringHelper,
                    CleanStringType.Ascii | CleanStringType.ConvertCase | CleanStringType.PascalCase));

            // TODO: If this still fails then we should get a list of properties from the object and then compare - doing the above without listing
            // all properties will surely be faster than using reflection to get ALL properties first and then query against them.
        }

        return attempt;
    }
}
