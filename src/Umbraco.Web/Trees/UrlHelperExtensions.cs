using System;
using System.Net.Http.Formatting;
using System.Web.Http.Routing;
using Umbraco.Core;

namespace Umbraco.Web.Trees
{
    public static class UrlHelperExtensions
    {
        public static string GetTreeUrl(this UrlHelper urlHelper, Type treeType, string nodeId, FormDataCollection queryStrings)
        {
            var actionUrl = urlHelper.GetUmbracoApiService("GetNodes", treeType)
                .EnsureEndsWith('?');
            
            //now we need to append the query strings
            actionUrl += "id=" + nodeId.EnsureEndsWith('&') + queryStrings.ToQueryString("id", 
                //Always ignore the custom start node id when generating URLs for tree nodes since this is a custom once-only parameter
                // that should only ever be used when requesting a tree to render (root), not a tree node 
                TreeQueryStringParameters.StartNodeId);
            return actionUrl;
        }

        public static string GetMenuUrl(this UrlHelper urlHelper, Type treeType, string nodeId, FormDataCollection queryStrings)
        {
            var actionUrl = urlHelper.GetUmbracoApiService("GetMenu", treeType)
                .EnsureEndsWith('?');

            //now we need to append the query strings
            actionUrl += "id=" + nodeId.EnsureEndsWith('&') + queryStrings.ToQueryString("id");
            return actionUrl;
        }

    }
}