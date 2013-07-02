using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an allowed section for a user
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class UserSection : IAggregateRoot
    {
        private int? _userId;
        private bool _hasIdentity;
        
        [DataMember]
        public string SectionAlias { get; set; }

        //NOTE: WE cannot implement these as the columns don't exist in the db.
        [IgnoreDataMember]
        Guid IEntity.Key { get; set; }
        [IgnoreDataMember]
        DateTime IEntity.CreateDate { get; set; }
        [IgnoreDataMember]
        DateTime IEntity.UpdateDate { get; set; }

        [IgnoreDataMember]
        public bool HasIdentity { get { return _userId != null || _hasIdentity; } }

        public int Id
        {
            get
            {
                return _userId.HasValue ? _userId.Value : -1;
            }
            set
            {
                _userId = value;
                _hasIdentity = true;
            }
        }
    }
}