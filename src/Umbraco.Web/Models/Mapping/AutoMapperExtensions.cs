using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal static class AutoMapperExtensions
    {
        /// <summary>
        /// This maps an object and passes in the current <see cref="UmbracoContext"/> so the mapping logic can use it
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="obj"></param>
        /// <param name="umbCtx"></param>
        /// <returns></returns>
        public static TOut MapWithUmbracoContext<TIn, TOut>(TIn obj, UmbracoContext umbCtx)
        {
            return Mapper.Map<TIn, TOut>(obj, opt => opt.Items["UmbracoContext"] = umbCtx);
        }
        
        /// <summary>
        /// Returns an <see cref="UmbracoContext"/> from the mapping options
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        /// <remarks>
        /// If an UmbracoContext is not found in the mapping options, it will try to retrieve it from the singleton
        /// </remarks>
        public static UmbracoContext GetUmbracoContext(this ResolutionContext res)
        {
            //get the context from the mapping options set during a mapping operation
            object umbCtx;
            if (res.Options.Items.TryGetValue("UmbracoContext", out umbCtx))
            {
                var umbracoContext = umbCtx as UmbracoContext;
                if (umbracoContext != null) return umbracoContext;
            }

            //return the singleton (this could be null)
            return UmbracoContext.Current;
        }
    }
}
