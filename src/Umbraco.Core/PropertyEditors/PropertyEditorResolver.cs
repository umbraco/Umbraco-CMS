using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core.IO;
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
            var builder = new ManifestBuilder(
                ApplicationContext.Current.ApplicationCache.RuntimeCache,
                new ManifestParser(new DirectoryInfo(IOHelper.MapPath("~/App_Plugins")), ApplicationContext.Current.ApplicationCache.RuntimeCache));

            _unioned = new Lazy<List<PropertyEditor>>(() => Values.Union(builder.PropertyEditors).ToList());
        }

        internal PropertyEditorResolver(Func<IEnumerable<Type>> typeListProducerList, ManifestBuilder builder)
            : base(typeListProducerList, ObjectLifetimeScope.Application)
        {
            _unioned = new Lazy<List<PropertyEditor>>(() => Values.Union(builder.PropertyEditors).ToList());
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