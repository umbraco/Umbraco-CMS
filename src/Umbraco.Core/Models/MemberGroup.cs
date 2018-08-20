using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a member type
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class MemberGroup : Entity, IMemberGroup
    {
        public MemberGroup()
        {
            AdditionalData = new Dictionary<string, object>();
        }

        private string _name;
        private int _creatorId;

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<MemberGroup, string>(x => x.Name);
            public readonly PropertyInfo CreatorIdSelector = ExpressionHelper.GetPropertyInfo<MemberGroup, int>(x => x.CreatorId);
        }

        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    //if the name has changed, add the value to the additional data,
                    //this is required purely for event handlers to know the previous name of the group
                    //so we can keep the public access up to date.
                    AdditionalData["previousName"] = _name;
                }

                SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector);                
            }
        }

        public int CreatorId
        {
            get { return _creatorId; }
            set { SetPropertyValueAndDetectChanges(value, ref _creatorId, Ps.Value.CreatorIdSelector); }
        }

        public IDictionary<string, object> AdditionalData { get; private set; }
    }
}