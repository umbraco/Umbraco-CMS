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

        private static readonly PropertyInfo RuleValueSelector = ExpressionHelper.GetPropertyInfo<PublicAccessRule, string>(x => x.RuleValue);
        private static readonly PropertyInfo RuleTypeSelector = ExpressionHelper.GetPropertyInfo<PublicAccessRule, string>(x => x.RuleType);
       
        public Guid AccessEntryId { get; internal set; }

        /// <summary>
        /// Method to call on entity saved when first added
        /// </summary>
        internal override void AddingEntity()
        {
            if (Key == default(Guid))
            {                
                Key = Guid.NewGuid();       
            }
            base.AddingEntity();
        }

        public string RuleValue
        {
            get { return _ruleValue; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _ruleValue = value;
                    return _ruleValue;
                }, _ruleValue, RuleValueSelector);
            }
        }

        public string RuleType
        {
            get { return _ruleType; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _ruleType = value;
                    return _ruleType;
                }, _ruleType, RuleTypeSelector);
            }
        }

        
    }
}