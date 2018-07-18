using System;
using System.Globalization;
using System.Reflection;
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
        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private string _isoCode;
        private string _cultureName;
        private bool _isDefaultVariantLanguage;
        private bool _mandatory;
        private int? _fallbackLanguageId;

        public Language(string isoCode)
        {
            IsoCode = isoCode;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo IsoCodeSelector = ExpressionHelper.GetPropertyInfo<Language, string>(x => x.IsoCode);
            public readonly PropertyInfo CultureNameSelector = ExpressionHelper.GetPropertyInfo<Language, string>(x => x.CultureName);
            public readonly PropertyInfo IsDefaultVariantLanguageSelector = ExpressionHelper.GetPropertyInfo<Language, bool>(x => x.IsDefault);
            public readonly PropertyInfo MandatorySelector = ExpressionHelper.GetPropertyInfo<Language, bool>(x => x.IsMandatory);
            public readonly PropertyInfo FallbackLanguageSelector = ExpressionHelper.GetPropertyInfo<Language, int?>(x => x.FallbackLanguageId);
        }

        /// <inheritdoc />
        [DataMember]
        public string IsoCode
        {
            get => _isoCode;
            set => SetPropertyValueAndDetectChanges(value, ref _isoCode, Ps.Value.IsoCodeSelector);
        }

        /// <inheritdoc />
        [DataMember]
        public string CultureName
        {
            get => _cultureName ?? CultureInfo.GetCultureInfo(IsoCode).DisplayName;
            set => SetPropertyValueAndDetectChanges(value, ref _cultureName, Ps.Value.CultureNameSelector);
        }

        /// <inheritdoc />
        [IgnoreDataMember]
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo(IsoCode);

        /// <inheritdoc />
        public bool IsDefault
        {
            get => _isDefaultVariantLanguage;
            set => SetPropertyValueAndDetectChanges(value, ref _isDefaultVariantLanguage, Ps.Value.IsDefaultVariantLanguageSelector);
        }

        /// <inheritdoc />
        public bool IsMandatory
        {
            get => _mandatory;
            set => SetPropertyValueAndDetectChanges(value, ref _mandatory, Ps.Value.MandatorySelector);
        }

        /// <inheritdoc />
        public int? FallbackLanguageId
        {
            get => _fallbackLanguageId;
            set => SetPropertyValueAndDetectChanges(value, ref _fallbackLanguageId, Ps.Value.FallbackLanguageSelector);
        }
    }
}
