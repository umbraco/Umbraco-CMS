using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    //TODO: Need to custom serialize this

    [Serializable]
    [DataContract(IsReference = true)]
    public class UmbracoDomain : Entity, IDomain
    {
        public UmbracoDomain(string domainName)
        {
            _domainName = domainName;
        }

        private IContent _content;
        private ILanguage _language;
        private string _domainName;

        private static readonly PropertyInfo DefaultLanguageSelector = ExpressionHelper.GetPropertyInfo<UmbracoDomain, ILanguage>(x => x.Language);
        private static readonly PropertyInfo DomainNameSelector = ExpressionHelper.GetPropertyInfo<UmbracoDomain, string>(x => x.DomainName);
        private static readonly PropertyInfo ContentSelector = ExpressionHelper.GetPropertyInfo<UmbracoDomain, IContent>(x => x.RootContent);

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
                }, _language, DefaultLanguageSelector);
            }
        }

        [DataMember]
        public string DomainName
        {
            get { return _domainName; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _domainName = value;
                    return _domainName;
                }, _domainName, DomainNameSelector);
            }
        }

        [DataMember]
        public IContent RootContent
        {
            get { return _content; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _content = value;
                    return _content;
                }, _content, ContentSelector);
            }
        }

        public bool IsWildcard
        {
            get { return string.IsNullOrWhiteSpace(DomainName) || DomainName.StartsWith("*"); }
        }
    }
}