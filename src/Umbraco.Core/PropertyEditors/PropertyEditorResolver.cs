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
    public class PropertyEditorResolver : LazyManyObjectsResolverBase<PropertyEditorResolver, PropertyEditor>
    {
        public PropertyEditorResolver(Func<IEnumerable<Type>> typeListProducerList)
            : base(typeListProducerList, ObjectLifetimeScope.Application)
        {
            _unioned = new Lazy<List<PropertyEditor>>(() => Values.Union(ManifestBuilder.PropertyEditors).ToList());
        }

        private readonly Lazy<List<PropertyEditor>> _unioned;

        /// <summary>
        /// Returns the property editors
        /// </summary>
        public IEnumerable<PropertyEditor> PropertyEditors
        {
            get { return _unioned.Value; }
        }

        /// <summary>
        /// Returns a property editor by alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public PropertyEditor GetByAlias(string alias)
        {
            return PropertyEditors.SingleOrDefault(x => x.Alias == alias);
        }
    }
}