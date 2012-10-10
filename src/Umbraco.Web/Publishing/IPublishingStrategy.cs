using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Web.Publishing
{
    public interface IPublishingStrategy
    {
        bool Publish(IContent content, int userId);
        bool PublishWithChildren(IEnumerable<IContent> children, int userId);
        void PublishWithSubs(IContent content, int userId);
        bool UnPublish(IContent content, int userId);
    }
}