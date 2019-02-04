using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class UmbracoDomain : EntityBase, IDomain
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

        [DataMember]
        public int? LanguageId
        {
            get => _languageId;
            set => SetPropertyValueAndDetectChanges(value, ref _languageId, nameof(LanguageId));
        }

        [DataMember]
        public string DomainName
        {
            get => _domainName;
            set => SetPropertyValueAndDetectChanges(value, ref _domainName, nameof(DomainName));
        }

        [DataMember]
        public int? RootContentId
        {
            get => _contentId;
            set => SetPropertyValueAndDetectChanges(value, ref _contentId, nameof(RootContentId));
        }

        public bool IsWildcard => string.IsNullOrWhiteSpace(DomainName) || DomainName.StartsWith("*");

        /// <summary>
        /// Readonly value of the language ISO code for the domain
        /// </summary>
        public string LanguageIsoCode { get; internal set; }
    }
}
