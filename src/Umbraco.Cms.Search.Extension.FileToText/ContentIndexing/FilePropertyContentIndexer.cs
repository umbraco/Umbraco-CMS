using Umbraco.Cms.Search.Extension.FileToText.FileIndexing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Search.Extension.FileToText.Configuration;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Extension.FileToText.ContentIndexing;

internal sealed class FilePropertyContentIndexer : IContentIndexer
{
    private readonly MediaFileManager _mediaFileManager;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IEnumerable<IFileValueHandler> _fileValueHandlers;
    private readonly ILogger<FilePropertyContentIndexer> _logger;
    private readonly IndexingOptions _indexingOptions;

    public FilePropertyContentIndexer(
        MediaFileManager mediaFileManager,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        IEnumerable<IFileValueHandler> fileValueHandlers,
        IOptions<IndexingOptions> indexingOptions,
        ILogger<FilePropertyContentIndexer> logger)
    {
        _mediaFileManager = mediaFileManager;
        _mediaUrlGenerators = mediaUrlGenerators;
        _fileValueHandlers = fileValueHandlers;
        _indexingOptions = indexingOptions.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<IndexField>> GetIndexFieldsAsync(
        IContentBase content,
        string?[] cultures,
        bool published,
        CancellationToken cancellationToken)
    {
        if ((content is IMedia && _indexingOptions.IncludeMedia is false)
            || (content is IContent && _indexingOptions.IncludeContent is false)
            || content is not (IMedia or IContent))
        {
            return [];
        }

        var indexFields = new List<IndexField>();

        foreach (IProperty property in content.Properties)
        {
            foreach (IPropertyValue propertyValue in property.Values)
            {
                if (cultures is not null
                    && propertyValue.Culture is not null
                    && cultures!.InvariantContains(propertyValue.Culture) is false)
                {
                    continue;
                }

                var value = published ? propertyValue.PublishedValue : propertyValue.EditedValue;
                if (_mediaUrlGenerators.TryGetMediaPath(property.PropertyType.PropertyEditorAlias, value, out var filePath) is false
                    || filePath.IsNullOrWhiteSpace())
                {
                    continue;
                }

                var extension = Path.GetExtension(filePath);
                if (extension.IsNullOrWhiteSpace())
                {
                    continue;
                }

                IFileValueHandler[] fileValueHandlers = _fileValueHandlers.Where(handler => handler.CanHandle(extension)).ToArray();
                if (fileValueHandlers.Length is 0)
                {
                    continue;
                }

                // if more than one handler can handle this extension, always prioritize custom handlers over the default ones
                IFileValueHandler fileValueHandler = fileValueHandlers.Length is 1
                    ? fileValueHandlers[0]
                    : fileValueHandlers.FirstOrDefault(handler => handler is not IDefaultFileValueHandler)
                      ?? fileValueHandlers.First();
                try
                {
                    if (_indexingOptions.MaxFileSize is not null
                        && _mediaFileManager.FileSystem.GetSize(filePath) > _indexingOptions.MaxFileSize * 1024)
                    {
                        _logger.LogInformation("Skipping file {filePath} on content with ID: {contentId} - the file exceeds the maximum file size of {maxFileSize} kb", filePath, content.Key, _indexingOptions.MaxFileSize);
                        continue;
                    }

                    await using Stream fs = _mediaFileManager.FileSystem.OpenFile(filePath);
                    var text = await fileValueHandler.GetFileContentsAsync(fs, cancellationToken);
                    if (text.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    indexFields.Add(
                        new(
                            FieldName: $"{property.Alias}{Constants.TextFieldPostfix}",
                            Value: new() { Texts = [text] },
                            Culture: propertyValue.Culture,
                            Segment: propertyValue.Segment
                        )
                    );
                }
                catch (Exception ex) when (ex is not OperationCanceledException || cancellationToken.IsCancellationRequested is false)
                {
                    _logger.LogError(ex, "Unable to extract text from content with ID: {contentId} (file value handler: {fileValueHandler})", content.Key, fileValueHandler.GetType().FullName);
                }
            }
        }

        return indexFields;
    }
}
