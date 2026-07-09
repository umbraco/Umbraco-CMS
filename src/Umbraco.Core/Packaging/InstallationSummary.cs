using System.Runtime.Serialization;
using System.Text;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     Represents a summary of items installed during a package installation.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class InstallationSummary
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallationSummary"/> class.
    /// </summary>
    /// <param name="packageName">The name of the package being installed.</param>
    public InstallationSummary(string packageName)
        => PackageName = packageName;

    /// <summary>
    ///     Gets the name of the package that was installed.
    /// </summary>
    public string PackageName { get; }

    /// <summary>
    ///     Gets or sets the warnings generated during installation.
    /// </summary>
    public InstallWarnings Warnings { get; set; } = new();

    /// <summary>
    ///     Gets or sets the collection of data types that were installed.
    /// </summary>
    public IEnumerable<IDataType> DataTypesInstalled { get; set; } = Enumerable.Empty<IDataType>();

    /// <summary>
    ///     Gets or sets the collection of languages that were installed.
    /// </summary>
    public IEnumerable<ILanguage> LanguagesInstalled { get; set; } = Enumerable.Empty<ILanguage>();

    /// <summary>
    ///     Gets or sets the collection of dictionary items that were installed.
    /// </summary>
    public IEnumerable<IDictionaryItem> DictionaryItemsInstalled { get; set; } = Enumerable.Empty<IDictionaryItem>();

    /// <summary>
    ///     Gets or sets the collection of templates that were installed.
    /// </summary>
    public IEnumerable<ITemplate> TemplatesInstalled { get; set; } = Enumerable.Empty<ITemplate>();

    /// <summary>
    ///     Gets or sets the collection of document types that were installed.
    /// </summary>
    public IEnumerable<IContentType> DocumentTypesInstalled { get; set; } = Enumerable.Empty<IContentType>();

    /// <summary>
    ///     Gets or sets the collection of media types that were installed.
    /// </summary>
    public IEnumerable<IMediaType> MediaTypesInstalled { get; set; } = Enumerable.Empty<IMediaType>();

    /// <summary>
    ///     Gets or sets the collection of stylesheets that were installed.
    /// </summary>
    public IEnumerable<IFile> StylesheetsInstalled { get; set; } = Enumerable.Empty<IFile>();

    /// <summary>
    ///     Gets or sets the collection of scripts that were installed.
    /// </summary>
    public IEnumerable<IScript> ScriptsInstalled { get; set; } = Enumerable.Empty<IScript>();

    /// <summary>
    ///     Gets or sets the collection of partial views that were installed.
    /// </summary>
    public IEnumerable<IPartialView> PartialViewsInstalled { get; set; } = Enumerable.Empty<IPartialView>();

    /// <summary>
    ///     Gets or sets the collection of content items that were installed.
    /// </summary>
    public IEnumerable<IContent> ContentInstalled { get; set; } = Enumerable.Empty<IContent>();

    /// <summary>
    ///     Gets or sets the collection of media items that were installed.
    /// </summary>
    public IEnumerable<IMedia> MediaInstalled { get; set; } = Enumerable.Empty<IMedia>();

    /// <summary>
    ///     Gets or sets the collection of entity containers that were installed.
    /// </summary>
    public IEnumerable<EntityContainer> EntityContainersInstalled { get; set; } = Enumerable.Empty<EntityContainer>();

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();

        void WriteConflicts<T>(IEnumerable<T>? source, Func<T, string?> selector, string message, bool appendLine = true)
        {
            var result = source?.Select(selector).ToList();
            if (result?.Count > 0)
            {
                sb.Append(message);
                sb.Append(string.Join(", ", result));

                if (appendLine)
                {
                    sb.AppendLine();
                }
            }
        }

        void WriteCount<T>(string message, IEnumerable<T> source, bool appendLine = true)
        {
            sb.Append(message);
            sb.Append(source?.Count() ?? 0);

            if (appendLine)
            {
                sb.AppendLine();
            }
        }

        WriteConflicts(Warnings?.ConflictingTemplates, x => x.Alias, "Conflicting templates found, they will be overwritten: ");
        WriteConflicts(Warnings?.ConflictingStylesheets, x => x?.Alias, "Conflicting stylesheets found, they will be overwritten: ");
        WriteCount("Data types installed: ", DataTypesInstalled);
        WriteCount("Languages installed: ", LanguagesInstalled);
        WriteCount("Dictionary items installed: ", DictionaryItemsInstalled);
        WriteCount("Templates installed: ", TemplatesInstalled);
        WriteCount("Document types installed: ", DocumentTypesInstalled);
        WriteCount("Media types installed: ", MediaTypesInstalled);
        WriteCount("Stylesheets installed: ", StylesheetsInstalled);
        WriteCount("Scripts installed: ", ScriptsInstalled);
        WriteCount("Partial views installed: ", PartialViewsInstalled);
        WriteCount("Entity containers installed: ", EntityContainersInstalled);
        WriteCount("Content items installed: ", ContentInstalled);
        WriteCount("Media items installed: ", MediaInstalled, false);

        return sb.ToString();
    }
}
