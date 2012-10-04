using System;
using System.Reflection;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
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

        public Language Language
        {
            get { return _language; }
            set
            {
                _language = value;
                OnPropertyChanged(LanguageSelector);
            }
        }

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