using System.Globalization;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a language.
/// </summary>
public interface ILanguage : IEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets or sets the ISO code of the language.
    /// </summary>
    [DataMember]
    string IsoCode { get; set; }

    /// <summary>
    ///     Gets or sets the culture name of the language.
    /// </summary>
    [DataMember]
    string CultureName { get; set; }

    /// <summary>
    ///     Gets the <see cref="CultureInfo" /> object for the language.
    /// </summary>
    [IgnoreDataMember]
    CultureInfo? CultureInfo { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the language is the default language.
    /// </summary>
    [DataMember]
    bool IsDefault { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the language is mandatory.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When a language is mandatory, a multi-lingual document cannot be published
    ///         without that language being published, and unpublishing that language unpublishes
    ///         the entire document.
    ///     </para>
    /// </remarks>
    [DataMember]
    bool IsMandatory { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of a fallback language.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The fallback language can be used in multi-lingual scenarios, to help
    ///         define fallback strategies when a value does not exist for a requested language.
    ///     </para>
    /// </remarks>
    [DataMember]
    int? FallbackLanguageId { get; set; }
}
