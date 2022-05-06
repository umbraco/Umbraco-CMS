// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

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

    public bool CreateTemplateForContentType
    {
        get
        {
            if (State?.TryGetValue(s_templateForContentTypeKey, out var result) ?? false)
            {
                if (result is not bool createTemplate)
                {
                    return false;
                }

                return createTemplate;
            }

            return false;
        }
        set
        {
            if (!value is bool && State is not null)
            {
                State[s_templateForContentTypeKey] = value;
            }
        }
    }

    public string? ContentTypeAlias
    {
        get
        {
            if (State?.TryGetValue(s_contentTypeAliasKey, out var result) ?? false)
            {
                return result as string;
            }

            return null;
        }

        set
        {
            if (value is not null && State is not null)
            {
                State[s_contentTypeAliasKey] = value;
            }
        }
    }
}
