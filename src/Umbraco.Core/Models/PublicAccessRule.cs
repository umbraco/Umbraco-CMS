using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class PublicAccessRule : EntityBase
    {
        private string _ruleValue;
        private string _ruleType;

        public PublicAccessRule(Guid id, Guid accessEntryId)
        {
            AccessEntryId = accessEntryId;
            Key = id;
            Id = Key.GetHashCode();
        }

        public PublicAccessRule()
        {
        }

        
        public Guid AccessEntryId { get; internal set; }

        public string RuleValue
        {
            get => _ruleValue;
            set => SetPropertyValueAndDetectChanges(value, ref _ruleValue, nameof(RuleValue));
        }

        public string RuleType
        {
            get => _ruleType;
            set => SetPropertyValueAndDetectChanges(value, ref _ruleType, nameof(RuleType));
        }


    }
}
