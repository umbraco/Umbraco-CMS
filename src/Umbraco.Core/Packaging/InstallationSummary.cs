using System.Runtime.Serialization;
using System.Text;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;

namespace Umbraco.Cms.Core.Packaging;

[Serializable]
[DataContract(IsReference = true)]
public class InstallationSummary
{
    public InstallationSummary(string packageName)
        => PackageName = packageName;

    public string PackageName { get; }

    public InstallWarnings Warnings { get; set; } = new();

    public IEnumerable<IDataType> DataTypesInstalled { get; set; } = Enumerable.Empty<IDataType>();

    public IEnumerable<ILanguage> LanguagesInstalled { get; set; } = Enumerable.Empty<ILanguage>();

    public IEnumerable<IDictionaryItem> DictionaryItemsInstalled { get; set; } = Enumerable.Empty<IDictionaryItem>();

    public IEnumerable<IMacro> MacrosInstalled { get; set; } = Enumerable.Empty<IMacro>();

    public IEnumerable<IPartialView> MacroPartialViewsInstalled { get; set; } = Enumerable.Empty<IPartialView>();

    public IEnumerable<ITemplate> TemplatesInstalled { get; set; } = Enumerable.Empty<ITemplate>();

    public IEnumerable<IContentType> DocumentTypesInstalled { get; set; } = Enumerable.Empty<IContentType>();

    public IEnumerable<IMediaType> MediaTypesInstalled { get; set; } = Enumerable.Empty<IMediaType>();

    public IEnumerable<IFile> StylesheetsInstalled { get; set; } = Enumerable.Empty<IFile>();

    public IEnumerable<IScript> ScriptsInstalled { get; set; } = Enumerable.Empty<IScript>();

    public IEnumerable<IPartialView> PartialViewsInstalled { get; set; } = Enumerable.Empty<IPartialView>();

    public IEnumerable<IContent> ContentInstalled { get; set; } = Enumerable.Empty<IContent>();

    public IEnumerable<IMedia> MediaInstalled { get; set; } = Enumerable.Empty<IMedia>();

    public IEnumerable<EntityContainer> EntityContainersInstalled { get; set; } = Enumerable.Empty<EntityContainer>();

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

        WriteConflicts(Warnings?.ConflictingMacros, x => x?.Alias, "Conflicting macros found, they will be overwritten: ");
        WriteConflicts(Warnings?.ConflictingTemplates, x => x.Alias, "Conflicting templates found, they will be overwritten: ");
        WriteConflicts(Warnings?.ConflictingStylesheets, x => x?.Alias, "Conflicting stylesheets found, they will be overwritten: ");
        WriteCount("Data types installed: ", DataTypesInstalled);
        WriteCount("Languages installed: ", LanguagesInstalled);
        WriteCount("Dictionary items installed: ", DictionaryItemsInstalled);
        WriteCount("Macros installed: ", MacrosInstalled);
        WriteCount("Macro partial views installed: ", MacroPartialViewsInstalled);
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
