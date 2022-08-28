// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.UploadField,
    "File upload",
    "fileupload",
    Group = Constants.PropertyEditors.Groups.Media,
    Icon = "icon-download-alt",
    ValueEditorIsReusable = true)]
public class FileUploadPropertyEditor : DataEditor, IMediaUrlGenerator,
    INotificationHandler<ContentCopiedNotification>, INotificationHandler<ContentDeletedNotification>,
    INotificationHandler<MediaDeletedNotification>, INotificationHandler<MediaSavingNotification>,
    INotificationHandler<MemberDeletedNotification>
{
    private readonly IContentService _contentService;
    private readonly IOptionsMonitor<ContentSettings> _contentSettings;
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly MediaFileManager _mediaFileManager;
    private readonly UploadAutoFillProperties _uploadAutoFillProperties;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public FileUploadPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        MediaFileManager mediaFileManager,
        IOptionsMonitor<ContentSettings> contentSettings,
        ILocalizedTextService localizedTextService,
        UploadAutoFillProperties uploadAutoFillProperties,
        IContentService contentService,
        IIOHelper ioHelper)
        : this(
            dataValueEditorFactory,
            mediaFileManager,
            contentSettings,
            localizedTextService,
            uploadAutoFillProperties,
            contentService,
            ioHelper,
            StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public FileUploadPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        MediaFileManager mediaFileManager,
        IOptionsMonitor<ContentSettings> contentSettings,
        ILocalizedTextService localizedTextService,
        UploadAutoFillProperties uploadAutoFillProperties,
        IContentService contentService,
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _mediaFileManager = mediaFileManager ?? throw new ArgumentNullException(nameof(mediaFileManager));
        _contentSettings = contentSettings;
        _localizedTextService = localizedTextService;
        _uploadAutoFillProperties = uploadAutoFillProperties;
        _contentService = contentService;
        _ioHelper = ioHelper;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

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

    public void Handle(ContentCopiedNotification notification)
    {
        // get the upload field properties with a value
        IEnumerable<IProperty> properties = notification.Original.Properties.Where(IsUploadField);

        // copy files
        var isUpdated = false;
        foreach (IProperty property in properties)
        {
            // copy each of the property values (variants, segments) to the destination
            foreach (IPropertyValue propertyValue in property.Values)
            {
                var propVal = property.GetValue(propertyValue.Culture, propertyValue.Segment);
                if (propVal == null || !(propVal is string str) || str.IsNullOrWhiteSpace())
                {
                    continue;
                }

                var sourcePath = _mediaFileManager.FileSystem.GetRelativePath(str);
                var copyPath = _mediaFileManager.CopyFile(notification.Copy, property.PropertyType, sourcePath);
                notification.Copy.SetValue(property.Alias, _mediaFileManager.FileSystem.GetUrl(copyPath),
                    propertyValue.Culture, propertyValue.Segment);
                isUpdated = true;
            }
        }

        // if updated, re-save the copy with the updated value
        if (isUpdated)
        {
            _contentService.Save(notification.Copy);
        }
    }

    public void Handle(ContentDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);

    public void Handle(MediaDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);

    public void Handle(MediaSavingNotification notification)
    {
        foreach (IMedia entity in notification.SavedEntities)
        {
            AutoFillProperties(entity);
        }
    }

    public void Handle(MemberDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new FileUploadConfigurationEditor(_ioHelper, _editorConfigurationParser);

    /// <summary>
    ///     Creates the corresponding property value editor.
    /// </summary>
    /// <returns>The corresponding property value editor.</returns>
    protected override IDataValueEditor CreateValueEditor()
    {
        FileUploadPropertyValueEditor editor = DataValueEditorFactory.Create<FileUploadPropertyValueEditor>(Attribute!);
        editor.Validators.Add(new UploadFileTypeValidator(_localizedTextService, _contentSettings));
        return editor;
    }

    /// <summary>
    ///     Gets a value indicating whether a property is an upload field.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>
    ///     <c>true</c> if the specified property is an upload field; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsUploadField(IProperty property) => property.PropertyType.PropertyEditorAlias ==
                                                             Constants.PropertyEditors.Aliases.UploadField;

    /// <summary>
    ///     The paths to all file upload property files contained within a collection of content entities
    /// </summary>
    /// <param name="entities"></param>
    private IEnumerable<string> ContainedFilePaths(IEnumerable<IContentBase> entities) => entities
        .SelectMany(x => x.Properties)
        .Where(IsUploadField)
        .SelectMany(GetFilePathsFromPropertyValues)
        .Distinct();

    /// <summary>
    ///     Look through all property values stored against the property and resolve any file paths stored
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    private IEnumerable<string> GetFilePathsFromPropertyValues(IProperty prop)
    {
        IReadOnlyCollection<IPropertyValue> propVals = prop.Values;
        foreach (IPropertyValue propertyValue in propVals)
        {
            // check if the published value contains data and return it
            var propVal = propertyValue.PublishedValue;
            if (propVal != null && propVal is string str1 && !str1.IsNullOrWhiteSpace())
            {
                yield return _mediaFileManager.FileSystem.GetRelativePath(str1);
            }

            // check if the edited value contains data and return it
            propVal = propertyValue.EditedValue;
            if (propVal != null && propVal is string str2 && !str2.IsNullOrWhiteSpace())
            {
                yield return _mediaFileManager.FileSystem.GetRelativePath(str2);
            }
        }
    }

    private void DeleteContainedFiles(IEnumerable<IContentBase> deletedEntities)
    {
        IEnumerable<string> filePathsToDelete = ContainedFilePaths(deletedEntities);
        _mediaFileManager.DeleteMediaFiles(filePathsToDelete);
    }

    /// <summary>
    ///     Auto-fill properties (or clear).
    /// </summary>
    private void AutoFillProperties(IContentBase model)
    {
        IEnumerable<IProperty> properties = model.Properties.Where(IsUploadField);

        foreach (IProperty property in properties)
        {
            ImagingAutoFillUploadField? autoFillConfig = _contentSettings.CurrentValue.GetConfig(property.Alias);
            if (autoFillConfig == null)
            {
                continue;
            }

            foreach (IPropertyValue pvalue in property.Values)
            {
                var svalue = property.GetValue(pvalue.Culture, pvalue.Segment) as string;
                if (string.IsNullOrWhiteSpace(svalue))
                {
                    _uploadAutoFillProperties.Reset(model, autoFillConfig, pvalue.Culture, pvalue.Segment);
                }
                else
                {
                    _uploadAutoFillProperties.Populate(model, autoFillConfig,
                        _mediaFileManager.FileSystem.GetRelativePath(svalue), pvalue.Culture, pvalue.Segment);
                }
            }
        }
    }
}
