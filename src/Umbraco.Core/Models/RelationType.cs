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
        private bool _isBidirectional;
        private Guid? _parentObjectType;
        private Guid? _childObjectType;

        [Obsolete("This constructor is no longer used and will be removed in future versions, use one of the other constructors instead")]
        public RelationType(string alias, string name)
            : this(name: name, alias: alias, false, null, null)
        {
        }

        public RelationType(string name, string alias, bool isBidrectional, Guid? parentObjectType, Guid? childObjectType)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(name));
            if (alias == null) throw new ArgumentNullException(nameof(alias));
            if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(alias));

            _name = name;
            _alias = alias;
            _isBidirectional = isBidrectional;
            _parentObjectType = parentObjectType;
            _childObjectType = childObjectType;
        }

        [Obsolete("This constructor is no longer used and will be removed in future versions, use one of the other constructors instead")]
        public RelationType(Guid childObjectType, Guid parentObjectType, string alias)
            : this(name: alias, alias: alias, isBidrectional: false, parentObjectType: parentObjectType, childObjectType: childObjectType)
        {   
        }

        [Obsolete("This constructor is no longer used and will be removed in future versions, use one of the other constructors instead")]
        public RelationType(Guid childObjectType, Guid parentObjectType, string alias, string name)
            : this(name: name, alias: alias, isBidrectional: false, parentObjectType: parentObjectType, childObjectType: childObjectType)
        {
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
            get => _isBidirectional;
            set => SetPropertyValueAndDetectChanges(value, ref _isBidirectional, nameof(IsBidirectional));
        }

        /// <summary>
        /// Gets or sets the Parents object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember]
        public Guid? ParentObjectType
        {
            get => _parentObjectType;
            set => SetPropertyValueAndDetectChanges(value, ref _parentObjectType, nameof(ParentObjectType));
        }

        /// <summary>
        /// Gets or sets the Childs object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember]
        public Guid? ChildObjectType
        {
            get => _childObjectType;
            set => SetPropertyValueAndDetectChanges(value, ref _childObjectType, nameof(ChildObjectType));
        }

    }
}
