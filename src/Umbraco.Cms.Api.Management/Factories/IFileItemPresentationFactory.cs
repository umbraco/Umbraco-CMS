using Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;
using Umbraco.Cms.Api.Management.ViewModels.Script.Item;
using Umbraco.Cms.Api.Management.ViewModels.StaticFile.Item;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Item;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentation models for file items.
/// </summary>
public interface IFileItemPresentationFactory
{
    /// <summary>
    /// Creates a collection of <see cref="PartialViewItemResponseModel"/> instances from the given paths.
    /// </summary>
    /// <param name="path">The collection of file paths to create response models for.</param>
    /// <returns>An enumerable of <see cref="PartialViewItemResponseModel"/> representing the file items.</returns>
    IEnumerable<PartialViewItemResponseModel> CreatePartialViewItemResponseModels(IEnumerable<string> path);

    /// <summary>
    /// Creates a collection of <see cref="ScriptItemResponseModel"/> instances from the given script file paths.
    /// </summary>
    /// <param name="paths">The collection of script file paths to create response models for.</param>
    /// <returns>An enumerable of <see cref="ScriptItemResponseModel"/> representing the script items.</returns>
    IEnumerable<ScriptItemResponseModel> CreateScriptItemResponseModels(IEnumerable<string> paths);

    /// <summary>
    /// Creates a collection of <see cref="StaticFileItemResponseModel"/> instances from the given file paths.
    /// </summary>
    /// <param name="paths">The collection of file paths to create response models for.</param>
    /// <returns>An enumerable of <see cref="StaticFileItemResponseModel"/> representing the static files.</returns>
    IEnumerable<StaticFileItemResponseModel> CreateStaticFileItemResponseModels(IEnumerable<string> paths);

    /// <summary>
    /// Creates <see cref="StylesheetItemResponseModel"/> instances for the specified stylesheet file paths.
    /// </summary>
    /// <param name="paths">The collection of stylesheet file paths.</param>
    /// <returns>An enumerable of <see cref="StylesheetItemResponseModel"/> representing the stylesheet items.</returns>
    IEnumerable<StylesheetItemResponseModel> CreateStylesheetItemResponseModels(IEnumerable<string> paths);
}
