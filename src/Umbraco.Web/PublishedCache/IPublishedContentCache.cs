using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Core.Models;

namespace Umbraco.Web.PublishedCache
{
    internal interface IPublishedContentCache : IPublishedCache
    {
        // do we want that one?
        IPublishedContent GetByRoute(UmbracoContext umbracoContext, string route, bool? hideTopLevelNode = null);
        // do we want that one?
        IPublishedContent GetByUrlAlias(UmbracoContext umbracoContext, int rootNodeId, string alias);

        // vs. having a GetDocumentByXPath ?!

        // do we want to get-by-xpath?

        bool HasContent(UmbracoContext umbracoContext);
    }
}
