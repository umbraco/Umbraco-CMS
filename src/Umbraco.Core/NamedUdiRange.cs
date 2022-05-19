namespace Umbraco.Cms.Core;

/// <summary>
///     Represents a <see cref="UdiRange" /> complemented with a name.
/// </summary>
public class NamedUdiRange : UdiRange
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NamedUdiRange" /> class  with a <see cref="Udi" /> and an optional
    ///     selector.
    /// </summary>
    /// <param name="udi">A <see cref="Udi" />.</param>
    /// <param name="selector">An optional selector.</param>
    public NamedUdiRange(Udi udi, string selector = Constants.DeploySelector.This)
        : base(udi, selector)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NamedUdiRange" /> class  with a <see cref="Udi" />, a name, and an
    ///     optional selector.
    /// </summary>
    /// <param name="udi">A <see cref="Udi" />.</param>
    /// <param name="name">A name.</param>
    /// <param name="selector">An optional selector.</param>
    public NamedUdiRange(Udi udi, string name, string selector = Constants.DeploySelector.This)
        : base(udi, selector) =>
        Name = name;

    /// <summary>
    ///     Gets or sets the name of the range.
    /// </summary>
    public string? Name { get; set; }
}
