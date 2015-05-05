using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core.LightInject;
using Umbraco.Core.Logging;
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
    public class PropertyEditorResolver : ContainerLazyManyObjectsResolver<PropertyEditorResolver, PropertyEditor>
    {
        private readonly ManifestBuilder _builder;

        internal PropertyEditorResolver(IServiceContainer container, ILogger logger, Func<IEnumerable<Type>> typeListProducerList, ManifestBuilder builder)
            : base(container, logger, typeListProducerList, ObjectLifetimeScope.Application)
        {
            _builder = builder;
            _unioned = new Lazy<List<PropertyEditor>>(() => Values.Union(builder.PropertyEditors).ToList());
        }

        private readonly Lazy<List<PropertyEditor>> _unioned;

        /// <summary>
        /// Returns the property editors
        /// </summary>
        public virtual IEnumerable<PropertyEditor> PropertyEditors
        {
            get { return _unioned.Value; }
        }

        /// <summary>
        /// Returns a property editor by alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public virtual PropertyEditor GetByAlias(string alias)
        {
            return PropertyEditors.SingleOrDefault(x => x.Alias == alias);
        }
    }
}