using System.Reflection;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Indicates that a composer should be disabled.
/// </summary>
/// <remarks>
///     <para>
///         If a type is specified, disables the composer of that type, else disables the composer marked with the
///         attribute.
///     </para>
///     <para>This attribute is *not* inherited.</para>
///     <para>This attribute applies to classes only, it is not possible to enable/disable interfaces.</para>
///     <para>
///         Assembly-level <see cref="DisableComposerAttribute" /> has greater priority than
///         <see cref="DisableAttribute" />
///         attribute when it is marking the composer itself, but lower priority that when it is referencing another
///         composer.
///     </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class DisableAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DisableAttribute" /> class.
    /// </summary>
    public DisableAttribute()
    {
    }

    public DisableAttribute(string fullTypeName, string assemblyName) =>
        DisabledType = Assembly.Load(assemblyName)?.GetType(fullTypeName);

    /// <summary>
    ///     Initializes a new instance of the <see cref="DisableAttribute" /> class.
    /// </summary>
    public DisableAttribute(Type disabledType) => DisabledType = disabledType;

    /// <summary>
    ///     Gets the disabled type, or null if it is the composer marked with the attribute.
    /// </summary>
    public Type? DisabledType { get; }
}
