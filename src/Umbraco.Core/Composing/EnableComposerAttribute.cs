namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Indicates that a composer should be enabled.
/// </summary>
/// <remarks>
///     <para>
///         If a type is specified, enables the composer of that type, else enables the composer marked with the
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
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class EnableComposerAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EnableComposerAttribute" /> class.
    /// </summary>
    public EnableComposerAttribute(Type enabledType) => EnabledType = enabledType;

    /// <summary>
    ///     Gets the enabled type, or null if it is the composer marked with the attribute.
    /// </summary>
    public Type EnabledType { get; }
}
