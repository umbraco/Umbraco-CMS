// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that is used to trigger the IFileService when the SaveTemplate method is called in the API, after the template has been saved.
/// </summary>
public class TemplateSavedNotification : SavedNotification<ITemplate>
{
    private const string TemplateForContentTypeKey = "CreateTemplateForContentType";
    private const string ContentTypeAliasKey = "ContentTypeAlias";

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateSavedNotification"/> class
    ///     with a single template.
    /// </summary>
    /// <param name="target">The template that was saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public TemplateSavedNotification(ITemplate target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateSavedNotification"/> class
    ///     with multiple templates.
    /// </summary>
    /// <param name="target">The templates that were saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public TemplateSavedNotification(IEnumerable<ITemplate> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the template was created for a content type.
    /// </summary>
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

    /// <summary>
    ///     Gets or sets the alias of the content type the template was created for.
    /// </summary>
    /// <remarks>
    ///     This is used when creating a document type with a template. It is not recommended to change or set this value.
    /// </remarks>
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
