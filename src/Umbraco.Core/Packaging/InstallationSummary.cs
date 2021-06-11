using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;

namespace Umbraco.Cms.Core.Packaging
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class InstallationSummary
    {
        public InstallationSummary(string packageName)
        {
            PackageName = packageName;
        }

        public InstallWarnings Warnings { get; set; } = new InstallWarnings();

        public IEnumerable<IDataType> DataTypesInstalled { get; set; } = Enumerable.Empty<IDataType>();
        public IEnumerable<ILanguage> LanguagesInstalled { get; set; } = Enumerable.Empty<ILanguage>();
        public IEnumerable<IDictionaryItem> DictionaryItemsInstalled { get; set; } = Enumerable.Empty<IDictionaryItem>();
        public IEnumerable<IMacro> MacrosInstalled { get; set; } = Enumerable.Empty<IMacro>();
        public IEnumerable<ITemplate> TemplatesInstalled { get; set; } = Enumerable.Empty<ITemplate>();
        public IEnumerable<IContentType> DocumentTypesInstalled { get; set; } = Enumerable.Empty<IContentType>();
        public IEnumerable<IMediaType> MediaTypesInstalled { get; set; } = Enumerable.Empty<IMediaType>();
        public IEnumerable<IFile> StylesheetsInstalled { get; set; } = Enumerable.Empty<IFile>();
        public IEnumerable<IContent> ContentInstalled { get; set; } = Enumerable.Empty<IContent>();
        public IEnumerable<IMedia> MediaInstalled { get; set; } = Enumerable.Empty<IMedia>();
        public string PackageName { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var macroConflicts = Warnings.ConflictingMacros.ToList();
            if (macroConflicts.Count > 0)
            {
                sb.Append("Conflicting macros found, they will be overwritten:");
                foreach(IMacro m in macroConflicts)
                {
                    sb.Append(m.Alias);
                    sb.Append(',');
                }
                sb.AppendLine(". ");
            }
            var templateConflicts = Warnings.ConflictingTemplates.ToList();
            if (templateConflicts.Count > 0)
            {
                sb.Append("Conflicting templates found, they will be overwritten:");
                foreach (IMacro m in templateConflicts)
                {
                    sb.Append(m.Alias);
                    sb.Append(',');
                }
                sb.AppendLine(". ");
            }
            var stylesheetConflicts = Warnings.ConflictingStylesheets.ToList();
            if (stylesheetConflicts.Count > 0)
            {
                sb.Append("Conflicting stylesheets found, they will be overwritten:");
                foreach (IMacro m in stylesheetConflicts)
                {
                    sb.Append(m.Alias);
                    sb.Append(',');
                }
                sb.AppendLine(". ");
            }

            sb.Append("Content items installed: ");
            sb.Append(ContentInstalled.Count());
            sb.AppendLine(". ");
            sb.Append("Media items installed: ");
            sb.Append(MediaInstalled.Count());
            sb.AppendLine(". ");
            sb.Append("Dictionary items installed: ");
            sb.Append(DictionaryItemsInstalled.Count());
            sb.AppendLine(". ");
            sb.Append("Macros installed: ");
            sb.Append(MacrosInstalled.Count());
            sb.AppendLine(". ");
            sb.Append("Stylesheets installed: ");
            sb.Append(StylesheetsInstalled.Count());
            sb.AppendLine(". ");
            sb.Append("Templates installed: ");
            sb.Append(TemplatesInstalled.Count());
            sb.AppendLine();
            sb.Append("Document types installed: ");
            sb.Append(DocumentTypesInstalled.Count());
            sb.AppendLine(". ");
            sb.Append("Media types installed: ");
            sb.Append(MediaTypesInstalled.Count());
            sb.AppendLine(". ");
            sb.Append("Data types items installed: ");
            sb.Append(DataTypesInstalled.Count());
            return sb.ToString();
        }
    }

}
