using System;
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
        private string _name;
        private int _creatorId;

        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<MemberGroup, string>(x => x.Name);
        private static readonly PropertyInfo CreatorIdSelector = ExpressionHelper.GetPropertyInfo<MemberGroup, int>(x => x.CreatorId);

        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _name = value;
                    return _name;
                }, _name, NameSelector);
            }
        }

        public int CreatorId
        {
            get { return _creatorId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _creatorId = value;
                    return _creatorId;
                }, _creatorId, CreatorIdSelector);
            }
        }

        /// <summary>
        /// Method to call when Entity is being saved
        /// </summary>
        /// <remarks>Created date is set and a Unique key is assigned</remarks>
        internal override void AddingEntity()
        {
            base.AddingEntity();

            if (Key == Guid.Empty)
                Key = Guid.NewGuid();
        }
    }
}