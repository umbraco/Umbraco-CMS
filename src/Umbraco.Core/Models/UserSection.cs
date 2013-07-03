using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models
{

    internal class UserSectionCollection : KeyedCollection<Tuple<object, string>, UserSection>, INotifyCollectionChanged
    {
        protected override Tuple<object, string> GetKeyForItem(UserSection item)
        {
            return new Tuple<object, string>(item.UserId, item.SectionAlias);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }

    /// <summary>
    /// Represents an allowed section for a user
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class UserSection : TracksChangesEntityBase
    {
        public UserSection(IUser user, string sectionAlias)
        {
            Mandate.ParameterNotNull(user, "user");
            Mandate.ParameterNotNullOrEmpty(sectionAlias, "sectionAlias");
            _userId = user.Id;
            _sectionAlias = sectionAlias;
        }

        private string _sectionAlias;
        private object _userId;

        private static readonly PropertyInfo SectionSelector = ExpressionHelper.GetPropertyInfo<UserSection, string>(x => x.SectionAlias);
        private static readonly PropertyInfo UserIdSelector = ExpressionHelper.GetPropertyInfo<UserSection, object>(x => x.UserId);

        /// <summary>
        /// Gets or sets the section alias
        /// </summary>
        [DataMember]
        public object UserId
        {
            get { return _userId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _userId = value;
                    return _userId;
                }, _userId, UserIdSelector);
            }
        }

        /// <summary>
        /// Gets or sets the section alias
        /// </summary>
        [DataMember]
        public string SectionAlias
        {
            get { return _sectionAlias; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _sectionAlias = value;
                    return _sectionAlias;
                }, _sectionAlias, SectionSelector);
            }
        }

        protected bool Equals(UserSection other)
        {
            return string.Equals(_sectionAlias, other._sectionAlias) && _userId.Equals(other._userId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UserSection) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_sectionAlias.GetHashCode()*397) ^ _userId.GetHashCode();
            }
        }
    }
}