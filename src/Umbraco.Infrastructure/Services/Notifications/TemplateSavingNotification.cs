// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Services.Notifications
{
    public class TemplateSavingNotification : SavingNotification<ITemplate>
    {
        private const string s_templateForContentTypeKey = "CreateTemplateForContentType";
        private const string s_contentTypeAliasKey = "ContentTypeAlias";

        public TemplateSavingNotification(ITemplate target, EventMessages messages) : base(target, messages)
        {
        }

        public TemplateSavingNotification(IEnumerable<ITemplate> target, EventMessages messages) : base(target, messages)
        {
        }

        public TemplateSavingNotification(ITemplate target, EventMessages messages, bool createTemplateForContentType,
            string contentTypeAlias) : base(target, messages)
        {
            CreateTemplateForContentType = createTemplateForContentType;
            ContentTypeAlias = contentTypeAlias;
        }

        public TemplateSavingNotification(IEnumerable<ITemplate> target, EventMessages messages,
            bool createTemplateForContentType,
            string contentTypeAlias) : base(target, messages)
        {
            CreateTemplateForContentType = createTemplateForContentType;
            ContentTypeAlias = contentTypeAlias;
        }

        public bool? CreateTemplateForContentType
        {
            get
            {
                State.TryGetValue(s_templateForContentTypeKey, out var result);
                return result as bool?;
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
