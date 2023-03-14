using System.Collections;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
/// Manages the built-in fallback policies.
/// </summary>
public struct Fallback : IEnumerable<int>
{
    /// <summary>
    /// Do not fallback.
    /// </summary>
    public const int None = 0;

    private readonly int[] _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="Fallback" /> struct with values.
    /// </summary>
    /// <param name="values">The values.</param>
    private Fallback(int[] values) => _values = values;

    /// <summary>
    /// Gets an ordered set of fallback policies.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>
    /// The fallback policy.
    /// </returns>
    public static Fallback To(params int[] values) => new(values);

    /// <summary>
    /// Fallback to the default value.
    /// </summary>
    public const int DefaultValue = 1;

    /// <summary>
    /// Fallback to other languages.
    /// </summary>
    public const int Language = 2;

    /// <summary>
    /// Fallback to tree ancestors.
    /// </summary>
    public const int Ancestors = 3;

    /// <summary>
    /// Fallback to the default language.
    /// </summary>
    public const int DefaultLanguage = 4;

    /// <summary>
    /// Gets the fallback to the default language policy.
    /// </summary>
    /// <value>
    /// The default language fallback policy.
    /// </value>
    public static Fallback ToDefaultLanguage => new Fallback(new[] { DefaultLanguage });

    /// <summary>
    /// Gets the fallback to the default value policy.
    /// </summary>
    /// <value>
    /// The default value fallback policy.
    /// </value>
    public static Fallback ToDefaultValue => new(new[] { DefaultValue });

    /// <summary>
    /// Gets the fallback to language policy.
    /// </summary>
    /// <value>
    /// The language fallback policy.
    /// </value>
    public static Fallback ToLanguage => new(new[] { Language });

    /// <summary>
    /// Gets the fallback to tree ancestors policy.
    /// </summary>
    /// <value>
    /// The tree ancestors fallback policy.
    /// </value>
    public static Fallback ToAncestors => new(new[] { Ancestors });

    /// <inheritdoc />
    public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)_values ?? Array.Empty<int>()).GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
