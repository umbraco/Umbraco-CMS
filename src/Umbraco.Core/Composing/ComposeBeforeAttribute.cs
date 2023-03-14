namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Indicates that a component is required by another composer.
/// </summary>
/// <remarks>
///     <para>
///         This attribute is *not* inherited. This means that a composer class inheriting from
///         another composer class does *not* inherit its requirements. However, the runtime checks
///         the *interfaces* of every composer for their requirements, so requirements declared on
///         interfaces are inherited by every composer class implementing the interface.
///     </para>
///     <para>
///         When targeting a class, indicates a dependency on the composer which must be enabled,
///         unless the requirement has explicitly been declared as weak (and then, only if the composer
///         is enabled).
///     </para>
///     <para>
///         When targeting an interface, indicates a dependency on enabled composers implementing
///         the interface. It could be no composer at all, unless the requirement has explicitly been
///         declared as strong (and at least one composer must be enabled).
///     </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class ComposeBeforeAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ComposeBeforeAttribute" /> class.
    /// </summary>
    /// <param name="requiringType">The type of the required composer.</param>
    public ComposeBeforeAttribute(Type requiringType)
    {
        if (typeof(IComposer).IsAssignableFrom(requiringType) == false)
        {
            throw new ArgumentException(
                $"Type {requiringType.FullName} is invalid here because it does not implement {typeof(IComposer).FullName}.");
        }

        RequiringType = requiringType;
    }

    /// <summary>
    ///     Gets the required type.
    /// </summary>
    public Type RequiringType { get; }
}
