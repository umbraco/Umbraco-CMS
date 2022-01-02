using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Web.UI.App_Plugins.Test
{
    public class DictionaryTestComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ContentApps().Append<DictionaryTestAppFactory>();
        }
    }

    public class DictionaryTestAppFactory : IContentAppFactory
    {
        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            if (source is IDictionaryItem)
            {
                return new ContentApp {Alias = "testDictionaryApp", Name = "Translator", Icon = "icon-umb-translation", View = "/App_Plugins/Test/testView.html"};
            }

            return null;
        }
    }
}
