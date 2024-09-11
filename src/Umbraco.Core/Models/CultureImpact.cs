using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the impact of a culture set.
/// </summary>
/// <remarks>
///     <para>
///         A set of cultures can be either all cultures (including the invariant culture), or
///         the invariant culture, or a specific culture.
///     </para>
/// </remarks>
public sealed class CultureImpact
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CultureImpact" /> class.
    /// </summary>
    /// <param name="culture">The culture code.</param>
    /// <param name="isDefault">A value indicating whether the culture is the default culture.</param>
    /// <param name="allowEditInvariantFromNonDefault">A value indicating if publishing invariant properties from non-default language.</param>
    internal CultureImpact(string? culture, bool isDefault = false, bool allowEditInvariantFromNonDefault = false)
    {
        if (culture != null && culture.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("Culture \"\" is not valid here.");
        }

        Culture = culture;

        if ((culture == null || culture == "*") && isDefault)
        {
            throw new ArgumentException("The invariant or 'all' culture can not be the default culture.");
        }

        ImpactsOnlyDefaultCulture = isDefault;

        AllowEditInvariantFromNonDefault = allowEditInvariantFromNonDefault;
    }

    [Flags]
    public enum Behavior : byte
    {
        AllCultures = 1,
        InvariantCulture = 2,
        ExplicitCulture = 4,
        InvariantProperties = 8,
    }

    /// <summary>
    ///     Gets the impact of 'all' cultures (including the invariant culture).
    /// </summary>
    public static CultureImpact All { get; } = new("*");

    /// <summary>
    ///     Gets the impact of the invariant culture.
    /// </summary>
    public static CultureImpact Invariant { get; } = new(null);

    /// <summary>
    ///     Gets the culture code.
    /// </summary>
    /// <remarks>
    ///     <para>Can be null (invariant) or * (all cultures) or a specific culture code.</para>
    /// </remarks>
    public string? Culture { get; }

    /// <summary>
    ///     Gets a value indicating whether this impact impacts all cultures, including,
    ///     indirectly, the invariant culture.
    /// </summary>
    public bool ImpactsAllCultures => Culture == "*";

    /// <summary>
    ///     Gets a value indicating whether this impact impacts only the invariant culture,
    ///     directly, not because all cultures are impacted.
    /// </summary>
    public bool ImpactsOnlyInvariantCulture => Culture == null;

    /// <summary>
    ///     Gets a value indicating whether this impact impacts an implicit culture.
    /// </summary>
    /// <remarks>
    ///     And then it does not impact the invariant culture. The impacted
    ///     explicit culture could be the default culture.
    /// </remarks>
    public bool ImpactsExplicitCulture => Culture != null && Culture != "*";

    /// <summary>
    ///     Gets a value indicating whether this impact impacts the default culture, directly,
    ///     not because all cultures are impacted.
    /// </summary>
    public bool ImpactsOnlyDefaultCulture { get; }

    /// <summary>
    ///     Gets a value indicating whether this impact impacts the invariant properties, either
    ///     directly, or because all cultures are impacted, or because the default culture is impacted.
    /// </summary>
    public bool ImpactsInvariantProperties => Culture == null || Culture == "*" || ImpactsOnlyDefaultCulture;

    /// <summary>
    ///     Gets a value indicating whether this also impact impacts the invariant properties,
    ///     even though it does not impact the invariant culture, neither directly (ImpactsInvariantCulture)
    ///     nor indirectly (ImpactsAllCultures).
    /// </summary>
    public bool ImpactsAlsoInvariantProperties => !ImpactsOnlyInvariantCulture &&
                                                  !ImpactsAllCultures &&
                                                  (ImpactsOnlyDefaultCulture || AllowEditInvariantFromNonDefault);

    public Behavior CultureBehavior
    {
        get
        {
            // null can only be invariant
            if (Culture == null)
            {
                return Behavior.InvariantCulture | Behavior.InvariantProperties;
            }

            // * is All which means its also invariant properties since this will include the default language
            if (Culture == "*")
            {
                return Behavior.AllCultures | Behavior.InvariantProperties;
            }

            // else it's explicit
            Behavior result = Behavior.ExplicitCulture;

            // if the explicit culture is the default, then the behavior is also InvariantProperties
            if (ImpactsOnlyDefaultCulture)
            {
                result |= Behavior.InvariantProperties;
            }

            return result;
        }
    }

    /// <summary>
    ///     Utility method to return the culture used for invariant property errors based on what cultures are being actively
    ///     saved,
    ///     the default culture and the state of the current content item
    /// </summary>
    /// <param name="content"></param>
    /// <param name="savingCultures"></param>
    /// <param name="defaultCulture"></param>
    /// <returns></returns>
    public static string? GetCultureForInvariantErrors(IContent? content, string?[] savingCultures,
        string? defaultCulture)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (savingCultures == null)
        {
            throw new ArgumentNullException(nameof(savingCultures));
        }

        if (savingCultures.Length == 0)
        {
            throw new ArgumentException(nameof(savingCultures));
        }

        var cultureForInvariantErrors = savingCultures.Any(x => x.InvariantEquals(defaultCulture))

            // the default culture is being flagged for saving so use it
            ? defaultCulture

            // If the content has no published version, we need to affiliate validation with the first variant being saved.
            // If the content has a published version we will not affiliate the validation with any culture (null)
            : !content.Published
                ? savingCultures[0]
                : null;

        return cultureForInvariantErrors;
    }

    /// <summary>
    ///     Creates an impact instance representing the impact of a specific culture.
    /// </summary>
    /// <param name="culture">The culture code.</param>
    /// <param name="isDefault">A value indicating whether the culture is the default culture.</param>
    [Obsolete("Use ICultureImpactService instead.")]
    public static CultureImpact Explicit(string? culture, bool isDefault)
    {
        if (culture == null)
        {
            throw new ArgumentException("Culture <null> is not explicit.");
        }

        if (culture.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("Culture \"\" is not explicit.");
        }

        if (culture == "*")
        {
            throw new ArgumentException("Culture \"*\" is not explicit.");
        }

        return new CultureImpact(culture, isDefault);
    }

    public bool AllowEditInvariantFromNonDefault { get; }
}
