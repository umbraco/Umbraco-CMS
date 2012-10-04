using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a translation for a <see cref="DictionaryItem"/>
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class DictionaryTranslation : Entity
    {
        private Language _language;
        private string _value;

        public DictionaryTranslation(Language language, string value)
        {
            _language = language;
            _value = value;
        }

        public DictionaryTranslation(Language language, string value, Guid uniqueId)
        {
            _language = language;
            _value = value;
            Key = uniqueId;
        }

        private static readonly PropertyInfo LanguageSelector = ExpressionHelper.GetPropertyInfo<DictionaryTranslation, Language>(x => x.Language);
        private static readonly PropertyInfo ValueSelector = ExpressionHelper.GetPropertyInfo<DictionaryTranslation, string>(x => x.Value);

        /// <summary>
        /// Gets or sets the <see cref="Language"/> for the translation
        /// </summary>
        [DataMember]
        public Language Language
        {
            get { return _language; }
            set
            {
                _language = value;
                OnPropertyChanged(LanguageSelector);
            }
        }

        /// <summary>
        /// Gets or sets the translated text
        /// </summary>
        [DataMember]
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged(ValueSelector);
            }
        }
    }
}