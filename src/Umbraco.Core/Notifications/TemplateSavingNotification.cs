// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     A notification that is used to trigger the IFileService when the SaveTemplate method is called in the API.
/// </summary>
/// <remarks>
///     This notification is cancelable, allowing handlers to prevent the save operation
///     by setting <see cref="ICancelableNotification.Cancel"/> to <c>true</c>.
/// </remarks>
public class TemplateSavingNotification : SavingNotification<ITemplate>
{
    private const string TemplateForContentTypeKey = "CreateTemplateForContentType";
    private const string ContentTypeAliasKey = "ContentTypeAlias";

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateSavingNotification"/> class
    ///     with a single template.
    /// </summary>
    /// <param name="target">The template being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public TemplateSavingNotification(ITemplate target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateSavingNotification"/> class
    ///     with multiple templates.
    /// </summary>
    /// <param name="target">The templates being saved.</param>
    /// <param name="messages">The event messages collection.</param>
    public TemplateSavingNotification(IEnumerable<ITemplate> target, EventMessages messages)
        : base(target, messages)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateSavingNotification"/>
    /// </summary>
    /// <param name="target">
    /// Initializes a new instance of the <see cref="ITemplate"/>.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    /// <param name="createTemplateForContentType">
    ///  Boolean value determining if the template is created for a Document Type. It's not recommended to change this value.
    /// </param>
    /// <param name="contentTypeAlias">
    /// This is the alias of the ContentType the template is for. This is used when creating a Document Type with Template. It's not recommended to try and change or set this.
    /// </param>
    public TemplateSavingNotification(ITemplate target, EventMessages messages, bool createTemplateForContentType, string contentTypeAlias)
        : base(target, messages)
    {
        CreateTemplateForContentType = createTemplateForContentType;
        ContentTypeAlias = contentTypeAlias;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateSavingNotification"/>
    /// </summary>
    /// <param name="target">
    /// Gets a enumeration of the <see cref="ITemplate"/>.
    /// </param>
    /// <param name="messages">
    /// Initializes a new instance of the <see cref="EventMessages"/>.
    /// </param>
    /// <param name="createTemplateForContentType">
    ///  Boolean value determining if the template is created for a Document Type. It's not recommended to change this value.
    /// </param>
    /// <param name="contentTypeAlias">
    /// This is the alias of the ContentType the template is for. This is used when creating a Document Type with Template. It's not recommended to try and change or set this.
    /// </param>
    public TemplateSavingNotification(IEnumerable<ITemplate> target, EventMessages messages, bool createTemplateForContentType, string contentTypeAlias)
        : base(target, messages)
    {
        CreateTemplateForContentType = createTemplateForContentType;
        ContentTypeAlias = contentTypeAlias;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the template is being created for a content type.
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
    ///     Gets or sets the alias of the content type the template is for.
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
