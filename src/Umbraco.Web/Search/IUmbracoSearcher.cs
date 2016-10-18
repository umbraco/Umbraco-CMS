using System.Collections.Generic;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Search
{
    public interface IUmbracoSearcher
    {
        IEnumerable<EntityBasic> Search(string query, UmbracoEntityTypes entityType, IUser user);

        IEnumerable<EntityBasic> Search(string query, UmbracoEntityTypes entityType, IUser user, string nodeTypeAlias);
    }
}
