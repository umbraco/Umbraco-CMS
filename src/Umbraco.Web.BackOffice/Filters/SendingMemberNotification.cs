using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.BackOffice.Filters
{
    public class SendingMemberNotification : INotification
    {
        public IUmbracoContext UmbracoContext { get; }

        public MemberDisplay Member { get; }

        public SendingMemberNotification(MemberDisplay member, IUmbracoContext umbracoContext)
        {
            Member = member;
            UmbracoContext = umbracoContext;
        }
    }
}
