namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Indicates that a composer should be disabled.
/// </summary>
/// <remarks>
///     <para>
///         Assembly-level <see cref="DisableComposerAttribute" /> has greater priority than
///         <see cref="DisableAttribute" />
///         attribute when it is marking the composer itself, but lower priority that when it is referencing another
///         composer.
///     </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DisableComposerAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DisableComposerAttribute" /> class.
    /// </summary>
    public DisableComposerAttribute(Type disabledType) => DisabledType = disabledType;

    /// <summary>
    ///     Gets the disabled type, or null if it is the composer marked with the attribute.
    /// </summary>
    public Type DisabledType { get; }
}
