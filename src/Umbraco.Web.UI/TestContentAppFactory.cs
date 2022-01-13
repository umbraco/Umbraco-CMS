using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Web.UI
{
    public class TestContentComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ContentApps().Append<TestContentAppFactory>();
        }
    }

    public class TestContentAppFactory : IContentAppFactory
    {
        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            if (source is not IUser && source is not IUserGroup)
                return null;

            return new ContentApp
            {
                Name = "2FA",
                Alias = "2fa",
                View = "/App_Plugins/Test/test.html",
                Icon = "icon-security-camera"
            };
        }
    }
}
