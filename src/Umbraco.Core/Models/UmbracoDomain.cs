using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class UmbracoDomain : Entity, IDomain
    {
        public UmbracoDomain(string domainName)
        {
            _domainName = domainName;
        }

        public UmbracoDomain(string domainName, string languageIsoCode)
            : this(domainName)
        {
            LanguageIsoCode = languageIsoCode;
        }

        private int? _contentId;
        private int? _languageId;
        private string _domainName;

        private static readonly PropertyInfo ContentSelector = ExpressionHelper.GetPropertyInfo<UmbracoDomain, int?>(x => x.RootContentId);
        private static readonly PropertyInfo DefaultLanguageSelector = ExpressionHelper.GetPropertyInfo<UmbracoDomain, int?>(x => x.LanguageId);
        private static readonly PropertyInfo DomainNameSelector = ExpressionHelper.GetPropertyInfo<UmbracoDomain, string>(x => x.DomainName);
        

        [DataMember]
        public int? LanguageId
        {
            get { return _languageId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _languageId = value;
                    return _languageId;
                }, _languageId, DefaultLanguageSelector);
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
        public int? RootContentId
        {
            get { return _contentId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _contentId = value;
                    return _contentId;
                }, _contentId, ContentSelector);
            }
        }

        public bool IsWildcard
        {
            get { return string.IsNullOrWhiteSpace(DomainName) || DomainName.StartsWith("*"); }
        }

        /// <summary>
        /// Readonly value of the language ISO code for the domain
        /// </summary>
        public string LanguageIsoCode { get; internal set; }
    }
}