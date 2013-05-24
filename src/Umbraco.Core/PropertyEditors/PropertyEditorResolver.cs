using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Manifest;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A resolver to resolve all property editors
    /// </summary>
    /// <remarks>
    /// This resolver will contain any property editors defined in manifests as well!
    /// </remarks>
    internal class PropertyEditorResolver : LazyManyObjectsResolverBase<PropertyEditorResolver, PropertyEditor>
    {
        public PropertyEditorResolver(Func<IEnumerable<Type>> typeListProducerList)
            : base(typeListProducerList, ObjectLifetimeScope.Application)
        {
        }

        /// <summary>
        /// Returns the property editors
        /// </summary>
        public IEnumerable<PropertyEditor> PropertyEditors
        {
            get { return Values.Union(ManifestBuilder.PropertyEditors); }
        }

        /// <summary>
        /// Returns a property editor by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PropertyEditor GetById(Guid id)
        {
            return PropertyEditors.SingleOrDefault(x => x.Id == id);
        }
    }
}