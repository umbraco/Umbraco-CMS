using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a member type
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class MemberGroup : EntityBase, IMemberGroup
    {
        private IDictionary<string, object> _additionalData;
        private string _name;
        private int _creatorId;

        /// <inheritdoc />
        [DataMember]
        [DoNotClone]
        public IDictionary<string, object> AdditionalData => _additionalData ?? (_additionalData = new Dictionary<string, object>());

        /// <inheritdoc />
        [IgnoreDataMember]
        public bool HasAdditionalData => _additionalData != null;

        [DataMember]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    //if the name has changed, add the value to the additional data,
                    //this is required purely for event handlers to know the previous name of the group
                    //so we can keep the public access up to date.
                    AdditionalData["previousName"] = _name;
                }

                SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
            }
        }

        [DataMember]
        public int CreatorId
        {
            get => _creatorId;
            set => SetPropertyValueAndDetectChanges(value, ref _creatorId, nameof(CreatorId));
        }
    }
}
