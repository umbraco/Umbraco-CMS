using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A resolver to resolve all parameter editors
    /// </summary>
    /// <remarks>
    /// This resolver will contain any parameter editors defined in manifests as well as any property editors defined in manifests
    /// that have the IsParameterEditorFlag = true and any PropertyEditors found in c# that have this flag as well. 
    /// </remarks>
    internal class ParameterEditorResolver : LazyManyObjectsResolverBase<ParameterEditorResolver, IParameterEditor>
    {
        private readonly ManifestBuilder _builder;
        
        public ParameterEditorResolver(IServiceProvider serviceProvider, ILogger logger, Func<IEnumerable<Type>> typeListProducerList, ManifestBuilder builder)
            : base(serviceProvider, logger, typeListProducerList, ObjectLifetimeScope.Application)
        {
            _builder = builder;
        }

        /// <summary>
        /// Returns the parameter editors
        /// </summary>
        public IEnumerable<IParameterEditor> ParameterEditors
        {
            get
            {
                //This will by default include all property editors and parameter editors but we need to filter this
                //list to ensure that none of the property editors that do not have the IsParameterEditor flag set to true 
                //are filtered.
                var filtered = Values.Select(x => x as PropertyEditor)
                                     .WhereNotNull()
                                     .Where(x => x.IsParameterEditor == false);
                
                return Values
                    //exclude the non parameter editor c# property editors
                    .Except(filtered)
                    //include the manifest parameter editors
                    .Union(_builder.ParameterEditors)
                    //include the manifest prop editors that are parameter editors
                    .Union(_builder.PropertyEditors.Where(x => x.IsParameterEditor));
            }
        }

        /// <summary>
        /// Returns a property editor by alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public IParameterEditor GetByAlias(string alias)
        {
            var found = ParameterEditors.SingleOrDefault(x => x.Alias == alias);
            if (found != null) return found;
            
            //couldn't find one, so try the map
            var mapped = LegacyParameterEditorAliasConverter.GetNewAliasFromLegacyAlias(alias);
            return mapped == null 
                ? null 
                : ParameterEditors.SingleOrDefault(x => x.Alias == mapped);
        }
    }
}