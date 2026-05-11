using System.Globalization;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.IO;

/// <summary>
/// Provides helper methods for working with template view files.
/// </summary>
public class ViewHelper : IViewHelper
{
    private readonly IDefaultViewContentProvider _defaultViewContentProvider;
    private readonly IFileSystem _viewFileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewHelper"/> class.
    /// </summary>
    /// <param name="fileSystems">The file systems provider.</param>
    /// <param name="defaultViewContentProvider">The default view content provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
    public ViewHelper(FileSystems fileSystems, IDefaultViewContentProvider defaultViewContentProvider)
        {
            _viewFileSystem = fileSystems.MvcViewsFileSystem ?? throw new ArgumentNullException(nameof(fileSystems));
            _defaultViewContentProvider = defaultViewContentProvider ?? throw new ArgumentNullException(nameof(defaultViewContentProvider));
    }

    /// <inheritdoc />
    public bool ViewExists(ITemplate t) => t.Alias is not null && _viewFileSystem.FileExists(ViewPath(t.Alias));

    /// <inheritdoc />
    public string GetFileContents(ITemplate t)
    {
        var viewContent = string.Empty;
        var path = ViewPath(t.Alias ?? string.Empty);

        if (_viewFileSystem.FileExists(path))
        {
            using (var tr = new StreamReader(_viewFileSystem.OpenFile(path)))
            {
                viewContent = tr.ReadToEnd();
                tr.Close();
            }
        }

        return viewContent;
    }

    /// <inheritdoc />
    public string CreateView(ITemplate t, bool overWrite = false)
    {
        string viewContent;
        var path = ViewPath(t.Alias);

        if (_viewFileSystem.FileExists(path) == false || overWrite)
        {
            viewContent = SaveTemplateToFile(t);
        }
        else
        {
            using (var tr = new StreamReader(_viewFileSystem.OpenFile(path)))
            {
                viewContent = tr.ReadToEnd();
                tr.Close();
            }
        }

        return viewContent;
    }

    /// <inheritdoc />
    public string? UpdateViewFile(ITemplate t, string? currentAlias = null)
    {
        var path = ViewPath(t.Alias);

        if (string.IsNullOrEmpty(currentAlias) == false && currentAlias != t.Alias)
        {
            // then kill the old file..
            var oldFile = ViewPath(currentAlias);
            if (_viewFileSystem.FileExists(oldFile))
            {
                _viewFileSystem.DeleteFile(oldFile);
            }
        }

        var data = Encoding.UTF8.GetBytes(t.Content ?? string.Empty);
        var withBom = Encoding.UTF8.GetPreamble().Concat(data).ToArray();

        using (var ms = new MemoryStream(withBom))
        {
            _viewFileSystem.AddFile(path, ms, true);
        }

        return t.Content;
    }

    /// <inheritdoc />
    public string ViewPath(string alias) => _viewFileSystem.GetRelativePath(alias.Replace(" ", string.Empty) + ".cshtml");

    /// <summary>
    /// Saves the template content to a view file.
    /// </summary>
    /// <param name="template">The template to save.</param>
    /// <returns>The content that was saved to the file.</returns>
    private string SaveTemplateToFile(ITemplate template)
    {
        var design = template.Content.IsNullOrWhiteSpace() ? EnsureInheritedLayout(template) : template.Content!;
        var path = ViewPath(template.Alias);

        var data = Encoding.UTF8.GetBytes(design);
        var withBom = Encoding.UTF8.GetPreamble().Concat(data).ToArray();

        using (var ms = new MemoryStream(withBom))
        {
            _viewFileSystem.AddFile(path, ms, true);
        }

        return design;
    }

    /// <summary>
    /// Ensures that the template has an inherited layout set.
    /// </summary>
    /// <param name="template">The template to check.</param>
    /// <returns>The template content with an inherited layout.</returns>
    private string EnsureInheritedLayout(ITemplate template)
    {
        var design = template.Content;

        if (string.IsNullOrEmpty(design))
        {
            design = _defaultViewContentProvider.GetDefaultFileContent(template.MasterTemplateAlias);
        }

        return design;
    }
}
