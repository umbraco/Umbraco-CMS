using System;
using System.Runtime.Serialization;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a RelationType
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class RelationType : EntityBase, IRelationType
    {
        private string _name;
        private string _alias;
        private bool _isBidrectional;
        private Guid _parentObjectType;
        private Guid _childObjectType;

        public RelationType(Guid childObjectType, Guid parentObjectType, string alias)
        {
            if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentNullOrEmptyException(nameof(alias));
            _childObjectType = childObjectType;
            _parentObjectType = parentObjectType;
            _alias = alias;
            Name = _alias;
        }

        public RelationType(Guid childObjectType, Guid parentObjectType, string alias, string name)
            : this(childObjectType, parentObjectType, alias)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullOrEmptyException(nameof(name));
            Name = name;
        }

        /// <summary>
        /// Gets or sets the Name of the RelationType
        /// </summary>
        [DataMember]
        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
        }

        /// <summary>
        /// Gets or sets the Alias of the RelationType
        /// </summary>
        [DataMember]
        public string Alias
        {
            get => _alias;
            set => SetPropertyValueAndDetectChanges(value, ref _alias, nameof(Alias));
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the RelationType is Bidirectional (true) or Parent to Child (false)
        /// </summary>
        [DataMember]
        public bool IsBidirectional
        {
            get => _isBidrectional;
            set => SetPropertyValueAndDetectChanges(value, ref _isBidrectional, nameof(IsBidirectional));
        }

        /// <summary>
        /// Gets or sets the Parents object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember]
        public Guid ParentObjectType
        {
            get => _parentObjectType;
            set => SetPropertyValueAndDetectChanges(value, ref _parentObjectType, nameof(ParentObjectType));
        }

        /// <summary>
        /// Gets or sets the Childs object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember]
        public Guid ChildObjectType
        {
            get => _childObjectType;
            set => SetPropertyValueAndDetectChanges(value, ref _childObjectType, nameof(ChildObjectType));
        }

    }
}
