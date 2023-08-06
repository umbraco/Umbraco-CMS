using System.Globalization;
using System.Runtime.Serialization;
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
    private string? _fallbackLanguageIsoCode;
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
    public string? FallbackIsoCode
    {
        get => _fallbackLanguageIsoCode;
        set => SetPropertyValueAndDetectChanges(value, ref _fallbackLanguageIsoCode, nameof(FallbackIsoCode));
    }
}
