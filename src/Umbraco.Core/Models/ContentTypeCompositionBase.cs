using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an abstract class for composition specific ContentType properties and methods
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class ContentTypeCompositionBase : ContentTypeBase, IContentTypeComposition
    {
        private List<IContentTypeComposition> _contentTypeComposition = new List<IContentTypeComposition>();
        internal List<int> RemovedContentTypeKeyTracker = new List<int>();

        protected ContentTypeCompositionBase(int parentId) : base(parentId)
        {
        }

        protected ContentTypeCompositionBase(IContentTypeComposition parent)
            : this(parent, null)
		{
		}

        protected ContentTypeCompositionBase(IContentTypeComposition parent, string alias)
            : base(parent, alias)
        {
            AddContentType(parent);
        }

        private static readonly PropertyInfo ContentTypeCompositionSelector =
            ExpressionHelper.GetPropertyInfo<ContentTypeCompositionBase, IEnumerable<IContentTypeComposition>>(
                x => x.ContentTypeComposition);

        /// <summary>
        /// List of ContentTypes that make up a composition of PropertyGroups and PropertyTypes for the current ContentType
        /// </summary>
        [DataMember]
        public IEnumerable<IContentTypeComposition> ContentTypeComposition
        {
            get { return _contentTypeComposition; }
        }

        /// <summary>
        /// Returns a list of <see cref="PropertyGroup"/> objects from the composition
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyGroup> CompositionPropertyGroups
        {
            get
            {
                var groups = ContentTypeComposition.SelectMany(x => x.CompositionPropertyGroups).Union(PropertyGroups);
                return groups;
            }
        }

        /// <summary>
        /// Returns a list of <see cref="PropertyType"/> objects from the composition
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyType> CompositionPropertyTypes
        {
            get
            {
                var propertyTypes = ContentTypeComposition.SelectMany(x => x.CompositionPropertyTypes).Union(PropertyTypes);
                return propertyTypes;
            }
        }

        /// <summary>
        /// Adds a new ContentType to the list of composite ContentTypes
        /// </summary>
        /// <param name="contentType"><see cref="ContentType"/> to add</param>
        /// <returns>True if ContentType was added, otherwise returns False</returns>
        public bool AddContentType(IContentTypeComposition contentType)
        {
            if (contentType.ContentTypeComposition.Any(x => x.CompositionAliases().Any(ContentTypeCompositionExists)))
                return false;

            if (string.IsNullOrEmpty(Alias) == false && Alias.Equals(contentType.Alias))
                return false;

            if (ContentTypeCompositionExists(contentType.Alias) == false)
            {
                //Before we actually go ahead and add the ContentType as a Composition we ensure that we don't
                //end up with duplicate PropertyType aliases - in which case we throw an exception.
                var conflictingPropertyTypeAliases = CompositionPropertyTypes.SelectMany(
                    x => contentType.CompositionPropertyTypes
                        .Where(y => y.Alias.Equals(x.Alias, StringComparison.InvariantCultureIgnoreCase))
                        .Select(p => p.Alias)).ToList();

                if (conflictingPropertyTypeAliases.Any())
                    throw new InvalidCompositionException
                          {
                              AddedCompositionAlias = contentType.Alias,
                              ContentTypeAlias = Alias,
                              PropertyTypeAlias =
                                  string.Join(", ", conflictingPropertyTypeAliases)
                          };

                _contentTypeComposition.Add(contentType);
                OnPropertyChanged(ContentTypeCompositionSelector);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a ContentType with the supplied alias from the the list of composite ContentTypes
        /// </summary>
        /// <param name="alias">Alias of a <see cref="ContentType"/></param>
        /// <returns>True if ContentType was removed, otherwise returns False</returns>
        public bool RemoveContentType(string alias)
        {
            if (ContentTypeCompositionExists(alias))
            {
                var contentTypeComposition = ContentTypeComposition.FirstOrDefault(x => x.Alias == alias);
                if (contentTypeComposition == null)//You can't remove a composition from another composition
                    return false;

                RemovedContentTypeKeyTracker.Add(contentTypeComposition.Id);
                
                //If the ContentType we are removing has Compositions of its own these needs to be removed as well
                var compositionIdsToRemove = contentTypeComposition.CompositionIds().ToList();
                if(compositionIdsToRemove.Any())
                    RemovedContentTypeKeyTracker.AddRange(compositionIdsToRemove);

                OnPropertyChanged(ContentTypeCompositionSelector);
                return _contentTypeComposition.Remove(contentTypeComposition);
            }
            return false;
        }

        /// <summary>
        /// Checks if a ContentType with the supplied alias exists in the list of composite ContentTypes
        /// </summary>
        /// <param name="alias">Alias of a <see cref="ContentType"/></param>
        /// <returns>True if ContentType with alias exists, otherwise returns False</returns>
        public bool ContentTypeCompositionExists(string alias)
        {
            if (ContentTypeComposition.Any(x => x.Alias.Equals(alias)))
                return true;

            if (ContentTypeComposition.Any(x => x.ContentTypeCompositionExists(alias)))
                return true;

            return false;
        }

        /// <summary>
        /// Checks whether a PropertyType with a given alias already exists
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns>Returns <c>True</c> if a PropertyType with the passed in alias exists, otherwise <c>False</c></returns>
        public override bool PropertyTypeExists(string propertyTypeAlias)
        {
            return CompositionPropertyTypes.Any(x => x.Alias == propertyTypeAlias);
        }

        /// <summary>
        /// Adds a PropertyGroup.
        /// This method will also check if a group already exists with the same name and link it to the parent.
        /// </summary>
        /// <param name="groupName">Name of the PropertyGroup to add</param>
        /// <returns>Returns <c>True</c> if a PropertyGroup with the passed in name was added, otherwise <c>False</c></returns>
        public override bool AddPropertyGroup(string groupName)
        {
            if (PropertyGroups.Any(x => x.Name == groupName))
                return false;

            var propertyGroup = new PropertyGroup {Name = groupName, SortOrder = 0};

            if (CompositionPropertyGroups.Any(x => x.Name == groupName))
            {
                var firstGroup = CompositionPropertyGroups.First(x => x.Name == groupName && x.ParentId.HasValue == false);
                propertyGroup.SetLazyParentId(new Lazy<int?>(() => firstGroup.Id));
            }

            if (PropertyGroups.Any())
            {
                var last = PropertyGroups.Last();
                propertyGroup.SortOrder = last.SortOrder + 1;
            }

            PropertyGroups.Add(propertyGroup);
            return true;
        }

        /// <summary>
        /// Adds a PropertyType to a specific PropertyGroup
        /// </summary>
        /// <param name="propertyType"><see cref="PropertyType"/> to add</param>
        /// <param name="propertyGroupName">Name of the PropertyGroup to add the PropertyType to</param>
        /// <returns>Returns <c>True</c> if PropertyType was added, otherwise <c>False</c></returns>
        public override bool AddPropertyType(PropertyType propertyType, string propertyGroupName)
        {
            if (PropertyTypeExists(propertyType.Alias) == false)
            {
                if (PropertyGroups.Contains(propertyGroupName))
                {
                    propertyType.PropertyGroupId = new Lazy<int>(() => PropertyGroups[propertyGroupName].Id);
                    PropertyGroups[propertyGroupName].PropertyTypes.Add(propertyType);
                }
                else
                {
                    //If the PropertyGroup doesn't already exist we create a new one 
                    var propertyTypes = new List<PropertyType> { propertyType };
                    var propertyGroup = new PropertyGroup(new PropertyTypeCollection(propertyTypes)) { Name = propertyGroupName, SortOrder = 1 };
                    //and check if its an inherited PropertyGroup, which exists in the composition
                    if (CompositionPropertyGroups.Any(x => x.Name == propertyGroupName))
                    {
                        var parentPropertyGroup = CompositionPropertyGroups.First(x => x.Name == propertyGroupName && x.ParentId.HasValue == false);
                        propertyGroup.SortOrder = parentPropertyGroup.SortOrder;
                        //propertyGroup.ParentId = parentPropertyGroup.Id;
                        propertyGroup.SetLazyParentId(new Lazy<int?>(() => parentPropertyGroup.Id));
                    }

                    PropertyGroups.Add(propertyGroup);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a list of ContentType aliases from the current composition 
        /// </summary>
        /// <returns>An enumerable list of string aliases</returns>
        /// <remarks>Does not contain the alias of the Current ContentType</remarks>
        public IEnumerable<string> CompositionAliases()
        {
            return ContentTypeComposition
                .Select(x => x.Alias)
                .Union(ContentTypeComposition.SelectMany(x => x.CompositionAliases()));
        }

        /// <summary>
        /// Gets a list of ContentType Ids from the current composition 
        /// </summary>
        /// <returns>An enumerable list of integer ids</returns>
        /// <remarks>Does not contain the Id of the Current ContentType</remarks>
        public IEnumerable<int> CompositionIds()
        {
            return ContentTypeComposition
                .Select(x => x.Id)
                .Union(ContentTypeComposition.SelectMany(x => x.CompositionIds()));
        }

        public override object DeepClone()
        {
            var clone = (ContentTypeCompositionBase)base.DeepClone();
            //turn off change tracking
            clone.DisableChangeTracking();
            //need to manually assign since this is an internal field and will not be automatically mapped
            clone.RemovedContentTypeKeyTracker = new List<int>();
            clone._contentTypeComposition = ContentTypeComposition.Select(x => (IContentTypeComposition)x.DeepClone()).ToList();
            //this shouldn't really be needed since we're not tracking
            clone.ResetDirtyProperties(false);
            //re-enable tracking
            clone.EnableChangeTracking();

            return clone;
        }
    }
}