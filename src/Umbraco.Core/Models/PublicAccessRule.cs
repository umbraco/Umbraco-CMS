using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class PublicAccessRule : Entity
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

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo RuleValueSelector = ExpressionHelper.GetPropertyInfo<PublicAccessRule, string>(x => x.RuleValue);
            public readonly PropertyInfo RuleTypeSelector = ExpressionHelper.GetPropertyInfo<PublicAccessRule, string>(x => x.RuleType);
        }
       
        public Guid AccessEntryId { get; internal set; }

        public string RuleValue
        {
            get { return _ruleValue; }
            set { SetPropertyValueAndDetectChanges(value, ref _ruleValue, Ps.Value.RuleValueSelector); }
        }

        public string RuleType
        {
            get { return _ruleType; }
            set { SetPropertyValueAndDetectChanges(value, ref _ruleType, Ps.Value.RuleTypeSelector); }
        }

        
    }
}