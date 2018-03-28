using System;
using AutoMapper;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Extends AutoMapper's <see cref="Mapper"/> class to handle Umbraco's context.
    /// </summary>
    internal static class ContextMapper
    {
        private const string UmbracoContextKey = "ContextMapper.UmbracoContext";

        public static TDestination Map<TSource, TDestination>(TSource obj, UmbracoContext umbracoContext)
            => Mapper.Map<TSource, TDestination>(obj, opt => opt.Items[UmbracoContextKey] = umbracoContext);

        public static UmbracoContext GetUmbracoContext(this ResolutionContext resolutionContext, bool throwIfMissing = true)
        {
            if (resolutionContext.Options.Items.TryGetValue(UmbracoContextKey, out var obj) && obj is UmbracoContext umbracoContext)
                return umbracoContext;

            // fixme - not a good idea at all
            // because this falls back to magic singletons
            // so really we should remove this line, but then some tests+app breaks ;(
            return Umbraco.Web.Composing.Current.UmbracoContext;

            // better fail fast
            if (throwIfMissing)
                throw new InvalidOperationException("AutoMapper ResolutionContext does not contain an UmbracoContext.");

            return null;
        }
    }
}
