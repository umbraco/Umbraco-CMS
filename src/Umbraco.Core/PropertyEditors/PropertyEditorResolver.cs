﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core.Cache;
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
    public class PropertyEditorResolver : LazyManyObjectsResolverBase<PropertyEditorResolver, PropertyEditor>
    {
        public PropertyEditorResolver(IServiceProvider serviceProvider, ILogger logger, Func<IEnumerable<Type>> typeListProducerList, IRuntimeCacheProvider runtimeCache)
            : base(serviceProvider, logger, typeListProducerList, ObjectLifetimeScope.Application)
        {
            var builder = new ManifestBuilder(
                runtimeCache,
                new ManifestParser(new DirectoryInfo(IOHelper.MapPath("~/App_Plugins")), runtimeCache));

            _unioned = new Lazy<List<PropertyEditor>>(() => Values.Union(builder.PropertyEditors).ToList());
        }

        [Obsolete("Use the ctor specifying all dependencies instead")]
        public PropertyEditorResolver(Func<IEnumerable<Type>> typeListProducerList)
            : base(typeListProducerList, ObjectLifetimeScope.Application)
        {
            var builder = new ManifestBuilder(
                ApplicationContext.Current.ApplicationCache.RuntimeCache,
                new ManifestParser(new DirectoryInfo(IOHelper.MapPath("~/App_Plugins")), ApplicationContext.Current.ApplicationCache.RuntimeCache));

            _unioned = new Lazy<List<PropertyEditor>>(() => Values.Union(builder.PropertyEditors).ToList());
        }

        internal PropertyEditorResolver(IServiceProvider serviceProvider, ILogger logger, Func<IEnumerable<Type>> typeListProducerList, ManifestBuilder builder)
            : base(serviceProvider, logger, typeListProducerList, ObjectLifetimeScope.Application)
        {
            _unioned = new Lazy<List<PropertyEditor>>(() => SanitizeNames(Values.Union(builder.PropertyEditors).ToList()));
        }

        private static List<PropertyEditor> SanitizeNames(List<PropertyEditor> editors)
        {
            var nestedContentEditorFromPackage = editors.FirstOrDefault(x => x.Alias == "Our.Umbraco.NestedContent");
            if (nestedContentEditorFromPackage != null)
            {
                nestedContentEditorFromPackage.Name = "(Obsolete) " + nestedContentEditorFromPackage.Name;
                nestedContentEditorFromPackage.IsDeprecated = true;
            }
            var multiUrlPickerEditorFromPackage = editors.FirstOrDefault(x => x.Alias == "RJP.MultiUrlPicker");
            if (multiUrlPickerEditorFromPackage != null)
            {
                multiUrlPickerEditorFromPackage.Name = "(Obsolete) " + multiUrlPickerEditorFromPackage.Name;
                multiUrlPickerEditorFromPackage.IsDeprecated = true;
            }
            return editors;

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
