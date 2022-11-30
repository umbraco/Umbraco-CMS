// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class TemplateSavedNotification : SavedNotification<ITemplate>
{
    private const string TemplateForContentTypeKey = "CreateTemplateForContentType";
    private const string ContentTypeAliasKey = "ContentTypeAlias";

    public TemplateSavedNotification(ITemplate target, EventMessages messages)
        : base(target, messages)
    {
    }

    public TemplateSavedNotification(IEnumerable<ITemplate> target, EventMessages messages)
        : base(target, messages)
    {
    }

    public bool CreateTemplateForContentType
    {
        get
        {
            if (State?.TryGetValue(TemplateForContentTypeKey, out var result) ?? false)
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
                State[TemplateForContentTypeKey] = value;
            }
        }
    }

    public string? ContentTypeAlias
    {
        get
        {
            if (State?.TryGetValue(ContentTypeAliasKey, out var result) ?? false)
            {
                return result as string;
            }

            return null;
        }

        set
        {
            if (value is not null && State is not null)
            {
                State[ContentTypeAliasKey] = value;
            }
        }
    }
}
