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

        public Language(string isoCode)
        {
            IsoCode = isoCode;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo IsoCodeSelector = ExpressionHelper.GetPropertyInfo<Language, string>(x => x.IsoCode);
            public readonly PropertyInfo CultureNameSelector = ExpressionHelper.GetPropertyInfo<Language, string>(x => x.CultureName);
            public readonly PropertyInfo IsDefaultVariantLanguageSelector = ExpressionHelper.GetPropertyInfo<Language, bool>(x => x.IsDefaultVariantLanguage);
            public readonly PropertyInfo MandatorySelector = ExpressionHelper.GetPropertyInfo<Language, bool>(x => x.Mandatory);
        }

        /// <summary>
        /// Gets or sets the Iso Code for the Language
        /// </summary>
        [DataMember]
        public string IsoCode
        {
            get => _isoCode;
            set => SetPropertyValueAndDetectChanges(value, ref _isoCode, Ps.Value.IsoCodeSelector);
        }

        /// <summary>
        /// Gets or sets the Culture Name for the Language
        /// </summary>
        [DataMember]
        public string CultureName
        {
            get => _cultureName;
            set => SetPropertyValueAndDetectChanges(value, ref _cultureName, Ps.Value.CultureNameSelector);
        }

        /// <summary>
        /// Returns a <see cref="CultureInfo"/> object for the current Language
        /// </summary>
        [IgnoreDataMember]
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo(IsoCode);

        public bool IsDefaultVariantLanguage
        {
            get => _isDefaultVariantLanguage;
            set => SetPropertyValueAndDetectChanges(value, ref _isDefaultVariantLanguage, Ps.Value.IsDefaultVariantLanguageSelector);
        }

        public bool Mandatory
        {
            get => _mandatory;
            set => SetPropertyValueAndDetectChanges(value, ref _mandatory, Ps.Value.MandatorySelector);
        }
    }
}
