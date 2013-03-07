using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Language
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [Mapper(typeof(LanguageMapper))]
    public class Language : Entity, ILanguage
    {
        private string _isoCode;
        private string _cultureName;

        public Language(string isoCode)
        {
            IsoCode = isoCode;
        }

        private static readonly PropertyInfo IsoCodeSelector = ExpressionHelper.GetPropertyInfo<Language, string>(x => x.IsoCode);
        private static readonly PropertyInfo CultureNameSelector = ExpressionHelper.GetPropertyInfo<Language, string>(x => x.CultureName);

        /// <summary>
        /// Gets or sets the Iso Code for the Language
        /// </summary>
        [DataMember]
        public string IsoCode
        {
            get { return _isoCode; }
            set
            {
                _isoCode = value;
                OnPropertyChanged(IsoCodeSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Culture Name for the Language
        /// </summary>
        [DataMember]
        public string CultureName
        {
            get { return _cultureName; }
            set
            {
                _cultureName = value;
                OnPropertyChanged(CultureNameSelector);
            }
        }

        /// <summary>
        /// Returns a <see cref="CultureInfo"/> object for the current Language
        /// </summary>
        [IgnoreDataMember]
        public CultureInfo CultureInfo
        {
            get { return CultureInfo.CreateSpecificCulture(IsoCode); }
        }
    }
}