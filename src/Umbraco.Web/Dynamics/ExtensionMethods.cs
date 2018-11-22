using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Umbraco.Web.Dynamics
{
    internal static class ExtensionMethods
    {
        public static DynamicPublishedContentList Random(this DynamicPublishedContentList source, int min, int max)
        {
            return Random(source, new Random().Next(min, max));
        }

        public static DynamicPublishedContentList Random(this DynamicPublishedContentList source, int max)
        {
            return new DynamicPublishedContentList(source.OrderByRandom().Take(max));
        }

        public static DynamicPublishedContent Random(this DynamicPublishedContentList source)
        {
            return new DynamicPublishedContent(source.OrderByRandom().First());
        }

        private static IEnumerable<IPublishedContent> OrderByRandom(this DynamicPublishedContentList source)
        {
            return source.Items.OrderBy(x => Guid.NewGuid());
        }

        public static DynamicPublishedContentList Children(this DynamicPublishedContent content)
        {
            return content.Children;
        }

        public static DynamicPublishedContentList Children(this DynamicPublishedContent content, string contentTypeAlias)
        {
            return new DynamicPublishedContentList(content.PublishedContent.Children().OfTypes(contentTypeAlias));
        }
    }
}
