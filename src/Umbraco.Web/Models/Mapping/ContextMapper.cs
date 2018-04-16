using System;
using System.Collections.Generic;
using AutoMapper;
using ClientDependency.Core;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Extends AutoMapper's <see cref="Mapper"/> class to handle Umbraco's context and other contextual data
    /// </summary>
    internal static class ContextMapper
    {
        public const string UmbracoContextKey = "ContextMapper.UmbracoContext";
        public const string LanguageKey = "ContextMapper.LanguageId";

        public static TDestination Map<TSource, TDestination>(TSource obj, UmbracoContext umbracoContext)
            => Mapper.Map<TSource, TDestination>(obj, opt => opt.Items[UmbracoContextKey] = umbracoContext);

        public static TDestination Map<TSource, TDestination>(TSource obj, UmbracoContext umbracoContext, IDictionary<string, object> contextVals)
            => Mapper.Map<TSource, TDestination>(obj, opt =>
            {
                //set the umb ctx
                opt.Items[UmbracoContextKey] = umbracoContext;
                //set other supplied context vals
                if (contextVals != null)
                {
                    foreach (var contextVal in contextVals)
                    {
                        opt.Items[contextVal.Key] = contextVal.Value;
                    }
                }
            });

        public static TDestination Map<TSource, TDestination>(TSource obj, UmbracoContext umbracoContext, object contextVals)
            => Mapper.Map<TSource, TDestination>(obj, opt =>
            {
                //set the umb ctx
                opt.Items[UmbracoContextKey] = umbracoContext;
                //set other supplied context vals
                if (contextVals != null)
                {
                    foreach (var contextVal in contextVals.ToDictionary())
                    {
                        opt.Items[contextVal.Key] = contextVal.Value;
                    }
                }
            });

        public static TDestination Map<TSource, TDestination>(TSource obj, IDictionary<string, object> contextVals)
            => Mapper.Map<TSource, TDestination>(obj, opt =>
            {
                //set other supplied context vals
                if (contextVals != null)
                {
                    foreach (var contextVal in contextVals)
                    {
                        opt.Items[contextVal.Key] = contextVal.Value;
                    }
                }
            });

        public static TDestination Map<TSource, TDestination>(TSource obj, object contextVals)
            => Mapper.Map<TSource, TDestination>(obj, opt =>
            {
                //set other supplied context vals
                if (contextVals != null)
                {
                    foreach (var contextVal in contextVals.ToDictionary())
                    {
                        opt.Items[contextVal.Key] = contextVal.Value;
                    }
                }
            });

        /// <summary>
        /// Returns the language Id in the mapping context if one is found
        /// </summary>
        /// <param name="resolutionContext"></param>
        /// <returns></returns>
        public static int? GetLanguageId(this ResolutionContext resolutionContext)
        {
            if (!resolutionContext.Options.Items.TryGetValue(LanguageKey, out var obj)) return null;

            if (obj is int i)
                return i;

            return null;
        }

        /// <summary>
        /// Returns the <see cref="UmbracoContext"/> in the mapping context if one is found
        /// </summary>
        /// <param name="resolutionContext"></param>
        /// <param name="throwIfMissing"></param>
        /// <returns></returns>
        public static UmbracoContext GetUmbracoContext(this ResolutionContext resolutionContext, bool throwIfMissing = true)
        {
            if (resolutionContext.Options.Items.TryGetValue(UmbracoContextKey, out var obj) && obj is UmbracoContext umbracoContext)
                return umbracoContext;

            // better fail fast
            if (throwIfMissing)
                throw new InvalidOperationException("AutoMapper ResolutionContext does not contain an UmbracoContext.");

            // fixme - not a good idea at all
            // because this falls back to magic singletons
            // so really we should remove this line, but then some tests+app breaks ;(
            return Umbraco.Web.Composing.Current.UmbracoContext;
        }
    }
}

