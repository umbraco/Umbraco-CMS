using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "propertyGroup", Namespace = "")]
    public class PropertyGroupDisplay<TPropertyTypeDisplay> : PropertyGroupBasic<TPropertyTypeDisplay>
        where TPropertyTypeDisplay : PropertyTypeDisplay
    {
        public PropertyGroupDisplay()
        {
            Properties = new List<TPropertyTypeDisplay>();
            ParentTabContentTypeNames = new List<string>();
            ParentTabContentTypes = new List<int>();
        }

        /// <summary>
        /// Gets the context content type.
        /// </summary>
        [DataMember(Name = "contentTypeId")]
        [ReadOnly(true)]
        public int ContentTypeId { get; set; }

        /// <summary>
        /// Gets the identifiers of the content types that define this group.
        /// </summary>
        [DataMember(Name = "parentTabContentTypes")]
        [ReadOnly(true)]
        public IEnumerable<int> ParentTabContentTypes { get; set; }

        /// <summary>
        /// Gets the name of the content types that define this group.
        /// </summary>
        [DataMember(Name = "parentTabContentTypeNames")]
        [ReadOnly(true)]
        public IEnumerable<string> ParentTabContentTypeNames { get; set; }
    }

    internal static class PropertyGroupDisplayExtensions
    {
        /// <summary>
        /// Orders the property groups by hierarchy (so child groups are after their parent group) and removes circular references.
        /// </summary>
        /// <param name="propertyGroups">The property groups.</param>
        /// <returns>
        /// The ordered property groups.
        /// </returns>
        public static IEnumerable<PropertyGroupDisplay<T>> OrderByHierarchy<T>(this IEnumerable<PropertyGroupDisplay<T>> propertyGroups)
            where T : PropertyTypeDisplay
        {
            var groups = propertyGroups.ToList();
            var visitedParentKeys = new HashSet<Guid>(groups.Count);

            IEnumerable<PropertyGroupDisplay<T>> OrderByHierarchy(Guid? parentKey)
            {
                if (parentKey.HasValue && visitedParentKeys.Add(parentKey.Value) == false)
                {
                    // We already visited this parent key, stop to prevent a circular reference
                    yield break;
                }

                foreach (var group in groups.Where(x => x.ParentKey == parentKey).OrderBy(x => x.Type).ThenBy(x => x.SortOrder))
                {
                    yield return group;

                    foreach (var childGroup in OrderByHierarchy(group.Key))
                    {
                        yield return childGroup;
                    }
                }
            }

            return OrderByHierarchy(null);
        }
    }
}
