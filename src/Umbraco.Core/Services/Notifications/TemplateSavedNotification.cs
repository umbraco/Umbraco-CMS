// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.Notifications
{
    public class TemplateSavedNotification : SavedNotification<ITemplate>
    {
        private const string s_templateForContentTypeKey = "CreateTemplateForContentType";
        private const string s_contentTypeAliasKey = "ContentTypeAlias";

        public TemplateSavedNotification(ITemplate target, EventMessages messages) : base(target, messages)
        {
        }

        public TemplateSavedNotification(IEnumerable<ITemplate> target, EventMessages messages) : base(target, messages)
        {
        }

        public bool CreateTemplateForContentType
        {
            get
            {
                State.TryGetValue(s_templateForContentTypeKey, out var result);

                if (result is not bool createTemplate)
                {
                    return false;
                }

                return createTemplate;
            }
            set => State[s_templateForContentTypeKey] = value;
        }

        public string ContentTypeAlias
        {
            get
            {
                State.TryGetValue(s_contentTypeAliasKey, out var result);
                return result as string;
            }
            set => State[s_contentTypeAliasKey] = value;
        }
    }
}
