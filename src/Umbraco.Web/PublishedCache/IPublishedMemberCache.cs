using System.Xml.XPath;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.PublishedCache
{
    public interface IPublishedMemberCache : IXPathNavigable
    {
        IPublishedContent GetByProviderKey(object key);
        IPublishedContent GetById(int memberId);
        IPublishedContent GetByUsername(string username);
        IPublishedContent GetByEmail(string email);
        IPublishedContent GetByMember(IMember member);

        XPathNavigator CreateNavigator(bool preview);

        // if the node does not exist, return null
        XPathNavigator CreateNodeNavigator(int id, bool preview);

        /// <summary>
        /// Gets a content type identified by its unique identifier.
        /// </summary>
        /// <param name="id">The content type unique identifier.</param>
        /// <returns>The content type, or null.</returns>
        IPublishedContentType GetContentType(int id);

        /// <summary>
        /// Gets a content type identified by its alias.
        /// </summary>
        /// <param name="alias">The content type alias.</param>
        /// <returns>The content type, or null.</returns>
        /// <remarks>The alias is case-insensitive.</remarks>
        IPublishedContentType GetContentType(string alias);
    }
}
