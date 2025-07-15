// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

// TODO (V17):
// - Remove the implementation of INotificationHandler as these have all been refactored out into sepate notification handler classes.
// - Remove the unused parameters from the constructor.

/// <summary>
/// Defines the file upload property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.UploadField,
    ValueEditorIsReusable = true)]
public class FileUploadPropertyEditor : DataEditor, IMediaUrlGenerator,
    INotificationHandler<ContentCopiedNotification>, INotificationHandler<ContentDeletedNotification>,
    INotificationHandler<MediaDeletedNotification>, INotificationHandler<MediaSavingNotification>,
    INotificationHandler<MemberDeletedNotification>
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadPropertyEditor"/> class.
    /// </summary>
    public FileUploadPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        MediaFileManager mediaFileManager,
        IOptionsMonitor<ContentSettings> contentSettings,
        UploadAutoFillProperties uploadAutoFillProperties,
        IContentService contentService,
        IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc/>
    public bool TryGetMediaPath(string? propertyEditorAlias, object? value, [MaybeNullWhen(false)] out string mediaPath)
    {
        if (propertyEditorAlias == Alias &&
            value?.ToString() is var mediaPathValue &&
            !string.IsNullOrWhiteSpace(mediaPathValue))
        {
            mediaPath = mediaPathValue;
            return true;
        }

        mediaPath = null;
        return false;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new FileUploadConfigurationEditor(_ioHelper);

    /// <summary>
    ///     Creates the corresponding property value editor.
    /// </summary>
    /// <returns>The corresponding property value editor.</returns>
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<FileUploadPropertyValueEditor>(Attribute!);

    #region Obsolete notification handler notifications

    /// <inheritdoc/>
    [Obsolete("This handler is no longer registered. Logic has been migrated to FileUploadContentCopiedNotificationHandler. Scheduled for removal in Umbraco 17.")]
    public void Handle(ContentCopiedNotification notification)
    {
        // This handler is no longer registered. Logic has been migrated to FileUploadContentCopiedNotificationHandler.
    }

    /// <inheritdoc/>
    [Obsolete("This handler is no longer registered. Logic has been migrated to FileUploadMediaSavingNotificationHandler. Scheduled for removal in Umbraco 17.")]
    public void Handle(MediaSavingNotification notification)
    {
        // This handler is no longer registered. Logic has been migrated to FileUploadMediaSavingNotificationHandler.
    }

    /// <inheritdoc/>
    [Obsolete("This handler is no longer registered. Logic has been migrated to FileUploadContentDeletedNotificationHandler. Scheduled for removal in Umbraco 17.")]
    public void Handle(ContentDeletedNotification notification)
    {
        // This handler is no longer registered. Logic has been migrated to FileUploadContentDeletedNotificationHandler.
    }


    /// <inheritdoc/>
    [Obsolete("This handler is no longer registered. Logic has been migrated to FileUploadMediaDeletedNotificationHandler. Scheduled for removal in Umbraco 17.")]
    public void Handle(MediaDeletedNotification notification)
    {
        // This handler is no longer registered. Logic has been migrated to FileUploadMediaDeletedNotificationHandler.
    }

    /// <inheritdoc/>
    [Obsolete("This handler is no longer registered. Logic has been migrated to FileUploadMemberDeletedNotificationHandler. Scheduled for removal in Umbraco 17.")]
    public void Handle(MemberDeletedNotification notification)
    {
        // This handler is no longer registered. Logic has been migrated to FileUploadMemberDeletedNotificationHandler.
    }

    #endregion
}
