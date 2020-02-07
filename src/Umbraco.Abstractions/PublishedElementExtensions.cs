using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods for <c>IPublishedElement</c>.
    /// </summary>
    public static class PublishedElementExtensions
    {
        #region OfTypes

        // the .OfType<T>() filter is nice when there's only one type
        // this is to support filtering with multiple types
        public static IEnumerable<T> OfTypes<T>(this IEnumerable<T> contents, params string[] types)
            where T : IPublishedElement
        {
            if (types == null || types.Length == 0) return Enumerable.Empty<T>();

            return contents.Where(x => types.InvariantContains(x.ContentType.Alias));
        }

        #endregion
    }
}
