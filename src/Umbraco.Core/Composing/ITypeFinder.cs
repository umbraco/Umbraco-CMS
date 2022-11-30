using System.Reflection;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Used to find objects by implemented types, names and/or attributes
/// </summary>
public interface ITypeFinder
{
    /// <summary>
    ///     Return a list of found local Assemblies that Umbraco should scan for type finding
    /// </summary>
    /// <value></value>
    IEnumerable<Assembly> AssembliesToScan { get; }

    Type? GetTypeByName(string name);

    /// <summary>
    ///     Finds any classes derived from the assignTypeFrom Type that contain the attribute TAttribute
    /// </summary>
    /// <param name="assignTypeFrom"></param>
    /// <param name="attributeType"></param>
    /// <param name="assemblies"></param>
    /// <param name="onlyConcreteClasses"></param>
    /// <returns></returns>
    IEnumerable<Type> FindClassesOfTypeWithAttribute(
        Type assignTypeFrom,
        Type attributeType,
        IEnumerable<Assembly>? assemblies = null,
        bool onlyConcreteClasses = true);

    /// <summary>
    ///     Returns all types found of in the assemblies specified of type T
    /// </summary>
    /// <param name="assignTypeFrom"></param>
    /// <param name="assemblies"></param>
    /// <param name="onlyConcreteClasses"></param>
    /// <returns></returns>
    IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly>? assemblies = null, bool onlyConcreteClasses = true);

    /// <summary>
    ///     Finds any classes with the attribute.
    /// </summary>
    /// <param name="attributeType">The attribute type </param>
    /// <param name="assemblies">The assemblies.</param>
    /// <param name="onlyConcreteClasses">if set to <c>true</c> only concrete classes.</param>
    /// <returns></returns>
    IEnumerable<Type> FindClassesWithAttribute(
        Type attributeType,
        IEnumerable<Assembly>? assemblies,
        bool onlyConcreteClasses);
}
