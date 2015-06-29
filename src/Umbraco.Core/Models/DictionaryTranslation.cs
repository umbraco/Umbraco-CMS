using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a translation for a <see cref="DictionaryItem"/>
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class DictionaryTranslation : Entity, IDictionaryTranslation
    {
        private ILanguage _language;
        private string _value;

        public DictionaryTranslation(ILanguage language, string value)
        {
            if (language == null) throw new ArgumentNullException("language");
            _language = language;
            _value = value;
        }

        public DictionaryTranslation(ILanguage language, string value, Guid uniqueId)
        {
            if (language == null) throw new ArgumentNullException("language");
            _language = language;
            _value = value;
            Key = uniqueId;
        }

        private static readonly PropertyInfo LanguageSelector = ExpressionHelper.GetPropertyInfo<DictionaryTranslation, ILanguage>(x => x.Language);
        private static readonly PropertyInfo ValueSelector = ExpressionHelper.GetPropertyInfo<DictionaryTranslation, string>(x => x.Value);

        /// <summary>
        /// Gets or sets the <see cref="Language"/> for the translation
        /// </summary>
        [DataMember]
        public ILanguage Language
        {
            get { return _language; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _language = value;
                    return _language;
                }, _language, LanguageSelector);
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
                SetPropertyValueAndDetectChanges(o =>
                {
                    _value = value;
                    return _value;
                }, _value, ValueSelector);
            }
        }

    }
}