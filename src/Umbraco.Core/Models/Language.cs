using System;
using System.Globalization;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Language.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Language : EntityBase, ILanguage
    {
        private string _isoCode;
        private string _cultureName;
        private bool _isDefaultVariantLanguage;
        private bool _mandatory;
        private int? _fallbackLanguageId;

        public Language(string isoCode, string cultureName)
        {
            IsoCode = isoCode;
            CultureName = cultureName;
        }

        [Obsolete("Use the constructor also specifying the culture name instead.")]
        public Language(string isoCode)
            : this(isoCode, isoCode)
        { }

        /// <inheritdoc />
        [DataMember]
        public string IsoCode
        {
            get => _isoCode;
            set => SetPropertyValueAndDetectChanges(value, ref _isoCode, nameof(IsoCode));
        }

        /// <inheritdoc />
        [DataMember]
        public string CultureName
        {
            get => _cultureName;
            set => SetPropertyValueAndDetectChanges(value, ref _cultureName, nameof(CultureName));
        }

        /// <inheritdoc />
        [IgnoreDataMember]
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo(IsoCode);

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
}
