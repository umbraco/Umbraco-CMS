using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
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
        private readonly IContentSection _contentSection;

        public ParameterEditorResolver(IServiceProvider serviceProvider, ILogger logger, Func<IEnumerable<Type>> typeListProducerList, ManifestBuilder builder)
            : base(serviceProvider, logger, typeListProducerList, ObjectLifetimeScope.Application)
        {
            _builder = builder;
            _contentSection = UmbracoConfig.For.UmbracoSettings().Content;
        }

        /// <summary>
        /// Returns the parameter editors
        /// </summary>
        public IEnumerable<IParameterEditor> ParameterEditors
        {
            get { return GetParameterEditors(); }
        }

        public IEnumerable<IParameterEditor> GetParameterEditors(bool includeDeprecated = false)
        {
            // all property editors and parameter editors
            // except property editors where !IsParameterEditor
            var values = Values
                .Where(x => x is PropertyEditor == false || ((PropertyEditor) x).IsParameterEditor);

            // union all manifest parameter editors
            values = values
                .Union(_builder.ParameterEditors);

            // union all manifest property editors where IsParameterEditor
            values = values
                .Union(_builder.PropertyEditors.Where(x => x.IsParameterEditor));

            if (includeDeprecated == false && _contentSection.ShowDeprecatedPropertyEditors == false)
            {
                // except deprecated property editors
                values = values
                    .Where(x => x is PropertyEditor == false || ((PropertyEditor) x).IsDeprecated == false);
            }

            return values;
        }

        /// <summary>
        /// Returns a property editor by alias
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="includeDeprecated"></param>
        /// <returns></returns>
        public IParameterEditor GetByAlias(string alias, bool includeDeprecated = false)
        {
            var paramEditors = GetParameterEditors(includeDeprecated).ToArray();
            var found = paramEditors.SingleOrDefault(x => x.Alias == alias);
            if (found != null) return found;

            //couldn't find one, so try the map
            var mapped = LegacyParameterEditorAliasConverter.GetNewAliasFromLegacyAlias(alias);
            return mapped == null
                ? null
                : paramEditors.SingleOrDefault(x => x.Alias == mapped);
        }
    }
}