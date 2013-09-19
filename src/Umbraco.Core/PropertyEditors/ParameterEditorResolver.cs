using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Manifest;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A resolver to resolve all parameter editors
    /// </summary>
    /// <remarks>
    /// This resolver will contain any property editors defined in manifests as well!
    /// </remarks>
    internal class ParameterEditorResolver : LazyManyObjectsResolverBase<ParameterEditorResolver, ParameterEditor>
    {
        public ParameterEditorResolver(Func<IEnumerable<Type>> typeListProducerList)
            : base(typeListProducerList, ObjectLifetimeScope.Application)
        {
        }

        /// <summary>
        /// Returns the property editors
        /// </summary>
        public IEnumerable<ParameterEditor> ParameterEditors
        {
            get { return Values.Union(ManifestBuilder.ParameterEditors); }
        }

        /// <summary>
        /// Returns a property editor by alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public ParameterEditor GetByAlias(string alias)
        {
            return ParameterEditors.SingleOrDefault(x => x.Alias == alias);
        }
    }
}