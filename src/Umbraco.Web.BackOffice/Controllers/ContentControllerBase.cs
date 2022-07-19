using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     An abstract base controller used for media/content/members to try to reduce code replication.
/// </summary>
[JsonDateTimeFormat]
public abstract class ContentControllerBase : BackOfficeNotificationsController
{
    private readonly ILogger<ContentControllerBase> _logger;
    private readonly IJsonSerializer _serializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentControllerBase" /> class.
    /// </summary>
    protected ContentControllerBase(
        ICultureDictionary cultureDictionary,
        ILoggerFactory loggerFactory,
        IShortStringHelper shortStringHelper,
        IEventMessagesFactory eventMessages,
        ILocalizedTextService localizedTextService,
        IJsonSerializer serializer)
    {
        CultureDictionary = cultureDictionary;
        LoggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<ContentControllerBase>();
        ShortStringHelper = shortStringHelper;
        EventMessages = eventMessages;
        LocalizedTextService = localizedTextService;
        _serializer = serializer;
    }

    /// <summary>
    ///     Gets the <see cref="ICultureDictionary" />
    /// </summary>
    protected ICultureDictionary CultureDictionary { get; }

    /// <summary>
    ///     Gets the <see cref="ILoggerFactory" />
    /// </summary>
    protected ILoggerFactory LoggerFactory { get; }

    /// <summary>
    ///     Gets the <see cref="IShortStringHelper" />
    /// </summary>
    protected IShortStringHelper ShortStringHelper { get; }

    /// <summary>
    ///     Gets the <see cref="IEventMessagesFactory" />
    /// </summary>
    protected IEventMessagesFactory EventMessages { get; }

    /// <summary>
    ///     Gets the <see cref="ILocalizedTextService" />
    /// </summary>
    protected ILocalizedTextService LocalizedTextService { get; }

    /// <summary>
    ///     Handles if the content for the specified ID isn't found
    /// </summary>
    /// <param name="id">The content ID to find</param>
    /// <param name="throwException">Whether to throw an exception</param>
    /// <returns>The error response</returns>
    protected NotFoundObjectResult HandleContentNotFound(object id)
    {
        ModelState.AddModelError("id", $"content with id: {id} was not found");
        NotFoundObjectResult errorResponse = NotFound(ModelState);


        return errorResponse;
    }

    /// <summary>
    ///     Maps the dto property values to the persisted model
    /// </summary>
    internal void MapPropertyValuesForPersistence<TPersisted, TSaved>(
        TSaved contentItem,
        ContentPropertyCollectionDto? dto,
        Func<TSaved, IProperty?, object?> getPropertyValue,
        Action<TSaved, IProperty?, object?> savePropertyValue,
        string? culture)
        where TPersisted : IContentBase
        where TSaved : IContentSave<TPersisted>
    {
        if (dto is null)
        {
            return;
        }

        // map the property values
        foreach (ContentPropertyDto propertyDto in dto.Properties)
        {
            // get the property editor
            if (propertyDto.PropertyEditor == null)
            {
                _logger.LogWarning("No property editor found for property {PropertyAlias}", propertyDto.Alias);
                continue;
            }

            // get the value editor
            // nothing to save/map if it is readonly
            IDataValueEditor valueEditor = propertyDto.PropertyEditor.GetValueEditor();
            if (valueEditor.IsReadOnly)
            {
                continue;
            }

            // get the property
            IProperty property = contentItem.PersistedContent.Properties[propertyDto.Alias]!;

            // prepare files, if any matching property and culture
            ContentPropertyFile[] files = contentItem.UploadedFiles
                .Where(x => x.PropertyAlias == propertyDto.Alias && x.Culture == propertyDto.Culture &&
                            x.Segment == propertyDto.Segment)
                .ToArray();

            foreach (ContentPropertyFile file in files)
            {
                file.FileName = file.FileName?.ToSafeFileName(ShortStringHelper);
            }


            // create the property data for the property editor
            var data = new ContentPropertyData(propertyDto.Value, propertyDto.DataType?.Configuration)
            {
                ContentKey = contentItem.PersistedContent!.Key,
                PropertyTypeKey = property.PropertyType.Key,
                Files = files
            };

            // let the editor convert the value that was received, deal with files, etc
            var value = valueEditor.FromEditor(data, getPropertyValue(contentItem, property));

            // set the value - tags are special
            TagsPropertyEditorAttribute? tagAttribute = propertyDto.PropertyEditor.GetTagAttribute();
            if (tagAttribute != null)
            {
                TagConfiguration? tagConfiguration =
                    ConfigurationEditor.ConfigurationAs<TagConfiguration>(propertyDto.DataType?.Configuration);
                if (tagConfiguration is not null && tagConfiguration.Delimiter == default)
                {
                    tagConfiguration.Delimiter = tagAttribute.Delimiter;
                }

                var tagCulture = property?.PropertyType.VariesByCulture() ?? false ? culture : null;
                property?.SetTagsValue(_serializer, value, tagConfiguration, tagCulture);
            }
            else
            {
                savePropertyValue(contentItem, property, value);
            }
        }
    }

    /// <summary>
    ///     A helper method to attempt to get the instance from the request storage if it can be found there,
    ///     otherwise gets it from the callback specified
    /// </summary>
    /// <typeparam name="TPersisted"></typeparam>
    /// <param name="getFromService"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This is useful for when filters have already looked up a persisted entity and we don't want to have
    ///     to look it up again.
    /// </remarks>
    protected TPersisted? GetObjectFromRequest<TPersisted>(Func<TPersisted> getFromService) =>
        // checks if the request contains the key and the item is not null, if that is the case, return it from the request, otherwise return
        // it from the callback
        HttpContext.Items.ContainsKey(typeof(TPersisted).ToString()) &&
        HttpContext.Items[typeof(TPersisted).ToString()] != null
            ? (TPersisted?)HttpContext.Items[typeof(TPersisted).ToString()]
            : getFromService();

    /// <summary>
    ///     Returns true if the action passed in means we need to create something new
    /// </summary>
    /// <param name="action">The content action</param>
    /// <returns>Returns true  if this is a creating action</returns>
    internal static bool IsCreatingAction(ContentSaveAction action) => action.ToString().EndsWith("New");

    /// <summary>
    ///     Adds a cancelled message to the display
    /// </summary>
    /// <param name="display"></param>
    /// <param name="messageArea"></param>
    /// <param name="messageAlias"></param>
    /// <param name="messageParams"></param>
    protected void AddCancelMessage(
        INotificationModel? display,
        string messageArea = "speechBubbles",
        string messageAlias = "operationCancelledText",
        string[]? messageParams = null)
    {
        // if there's already a default event message, don't add our default one
        IEventMessagesFactory messages = EventMessages;
        if (messages != null && (messages.GetOrDefault()?.GetAll().Any(x => x.IsDefaultEventMessage) ?? false))
        {
            return;
        }

        display?.AddWarningNotification(
            LocalizedTextService.Localize("speechBubbles", "operationCancelledHeader"),
            LocalizedTextService.Localize(messageArea, messageAlias, messageParams));
    }

    /// <summary>
    ///     Adds a cancelled message to the display
    /// </summary>
    /// <param name="display"></param>
    /// <param name="header"></param>
    /// <param name="message"></param>
    /// <param name="headerArea"></param>
    /// <param name="headerAlias"></param>
    /// <param name="headerParams"></param>
    protected void AddCancelMessage(INotificationModel display, string message)
    {
        // if there's already a default event message, don't add our default one
        IEventMessagesFactory messages = EventMessages;
        if (messages?.GetOrDefault()?.GetAll().Any(x => x.IsDefaultEventMessage) == true)
        {
            return;
        }

        display.AddWarningNotification(LocalizedTextService.Localize("speechBubbles", "operationCancelledHeader"),
            message);
    }
}
