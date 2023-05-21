namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Indicates that a composer requires another composer.
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
public sealed class ComposeAfterAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ComposeAfterAttribute" /> class.
    /// </summary>
    /// <param name="requiredType">The type of the required composer.</param>
    public ComposeAfterAttribute(Type requiredType)
    {
        if (typeof(IComposer).IsAssignableFrom(requiredType) == false)
        {
            throw new ArgumentException(
                $"Type {requiredType.FullName} is invalid here because it does not implement {typeof(IComposer).FullName}.");
        }

        RequiredType = requiredType;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ComposeAfterAttribute" /> class.
    /// </summary>
    /// <param name="requiredType">The type of the required composer.</param>
    /// <param name="weak">A value indicating whether the requirement is weak.</param>
    public ComposeAfterAttribute(Type requiredType, bool weak)
        : this(requiredType) =>
        Weak = weak;

    /// <summary>
    ///     Gets the required type.
    /// </summary>
    public Type RequiredType { get; }

    /// <summary>
    ///     Gets a value indicating whether the requirement is weak.
    /// </summary>
    /// <remarks>
    ///     Returns <c>true</c> if the requirement is weak (requires the other composer if it
    ///     is enabled), <c>false</c> if the requirement is strong (requires the other composer to be
    ///     enabled), and <c>null</c> if unspecified, in which case it is strong for classes and weak for
    ///     interfaces.
    /// </remarks>
    public bool? Weak { get; }
}
