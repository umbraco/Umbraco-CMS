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
        private string _claim;
        private string _claimType;

        public PublicAccessRule(Guid id, Guid accessEntryId)
        {
            AccessEntryId = accessEntryId;
            Key = id;
            Id = Key.GetHashCode();
        }

        public PublicAccessRule()
        {
        }

        private static readonly PropertyInfo ClaimSelector = ExpressionHelper.GetPropertyInfo<PublicAccessRule, string>(x => x.Claim);
        private static readonly PropertyInfo ClaimTypeSelector = ExpressionHelper.GetPropertyInfo<PublicAccessRule, string>(x => x.ClaimType);

        public sealed override Guid Key
        {
            get { return base.Key; }
            set
            {
                base.Key = value;
                HasIdentity = true;
            }
        }

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

        public string Claim
        {
            get { return _claim; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _claim = value;
                    return _claim;
                }, _claim, ClaimSelector);
            }
        }

        public string ClaimType
        {
            get { return _claimType; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _claimType = value;
                    return _claimType;
                }, _claimType, ClaimTypeSelector);
            }
        }

        
    }
}