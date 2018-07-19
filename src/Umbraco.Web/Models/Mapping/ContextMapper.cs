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
        //public const string UmbracoContextKey = "ContextMapper.UmbracoContext";
        public const string CultureKey = "ContextMapper.Culture";
        
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
        public static string GetCulture(this ResolutionContext resolutionContext)
        {
            if (!resolutionContext.Options.Items.TryGetValue(CultureKey, out var obj)) return null;

            if (obj is string s)
                return s;

            return null;
        }
    }
}

