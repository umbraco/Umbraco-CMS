using System.Reflection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Extensions;

public static class TypeFinderExtensions
{
    /// <summary>
    ///     Finds any classes derived from the type T that contain the attribute TAttribute
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="typeFinder"></param>
    /// <param name="assemblies"></param>
    /// <param name="onlyConcreteClasses"></param>
    /// <returns></returns>
    public static IEnumerable<Type> FindClassesOfTypeWithAttribute<T, TAttribute>(
        this ITypeFinder typeFinder,
        IEnumerable<Assembly>? assemblies = null,
        bool onlyConcreteClasses = true)
        where TAttribute : Attribute
        => typeFinder.FindClassesOfTypeWithAttribute(typeof(T), typeof(TAttribute), assemblies, onlyConcreteClasses);

    /// <summary>
    ///     Returns all types found of in the assemblies specified of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="typeFinder"></param>
    /// <param name="assemblies"></param>
    /// <param name="onlyConcreteClasses"></param>
    /// <returns></returns>
    public static IEnumerable<Type> FindClassesOfType<T>(
        this ITypeFinder typeFinder,
        IEnumerable<Assembly>? assemblies = null,
        bool onlyConcreteClasses = true)
        => typeFinder.FindClassesOfType(typeof(T), assemblies, onlyConcreteClasses);

    /// <summary>
    ///     Finds the classes with attribute.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="typeFinder"></param>
    /// <param name="assemblies">The assemblies.</param>
    /// <param name="onlyConcreteClasses">if set to <c>true</c> only concrete classes.</param>
    /// <returns></returns>
    public static IEnumerable<Type> FindClassesWithAttribute<T>(
        this ITypeFinder typeFinder,
        IEnumerable<Assembly>? assemblies = null,
        bool onlyConcreteClasses = true)
        where T : Attribute
        => typeFinder.FindClassesWithAttribute(typeof(T), assemblies, onlyConcreteClasses);
}
