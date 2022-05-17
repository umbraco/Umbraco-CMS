using System.Globalization;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Language.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class Language : EntityBase, ILanguage
{
    private string _cultureName;
    private int? _fallbackLanguageId;
    private bool _isDefaultVariantLanguage;
    private string _isoCode;
    private bool _mandatory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Language" /> class.
    /// </summary>
    /// <param name="isoCode">The ISO code of the language.</param>
    /// <param name="cultureName">The name of the language.</param>
    public Language(string isoCode, string cultureName)
    {
        _isoCode = isoCode ?? throw new ArgumentNullException(nameof(isoCode));
        _cultureName = cultureName ?? throw new ArgumentNullException(nameof(cultureName));
    }

    [Obsolete(
        "Use the constructor not requiring global settings and accepting an explicit name instead, scheduled for removal in V11.")]
    public Language(GlobalSettings globalSettings, string isoCode)
    {
        _isoCode = isoCode ?? throw new ArgumentNullException(nameof(isoCode));
        _cultureName = CultureInfo.GetCultureInfo(isoCode).EnglishName;
    }

    /// <inheritdoc />
    [DataMember]
    public string IsoCode
    {
        get => _isoCode;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            SetPropertyValueAndDetectChanges(value, ref _isoCode!, nameof(IsoCode));
        }
    }

    /// <inheritdoc />
    [DataMember]
    public string CultureName
    {
        get => _cultureName;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            SetPropertyValueAndDetectChanges(value, ref _cultureName!, nameof(CultureName));
        }
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public CultureInfo? CultureInfo => IsoCode is not null ? CultureInfo.GetCultureInfo(IsoCode) : null;

    /// <inheritdoc />
    public bool IsDefault
    {
        get => _isDefaultVariantLanguage;
        set => SetPropertyValueAndDetectChanges(value, ref _isDefaultVariantLanguage, nameof(IsDefault));
    }

    /// <inheritdoc />
    public bool IsMandatory
    {
        get => _mandatory;
        set => SetPropertyValueAndDetectChanges(value, ref _mandatory, nameof(IsMandatory));
    }

    /// <inheritdoc />
    public int? FallbackLanguageId
    {
        get => _fallbackLanguageId;
        set => SetPropertyValueAndDetectChanges(value, ref _fallbackLanguageId, nameof(FallbackLanguageId));
    }
}
