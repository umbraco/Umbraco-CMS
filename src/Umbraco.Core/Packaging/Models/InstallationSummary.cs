using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models;

namespace Umbraco.Core.Packaging.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    internal class InstallationSummary
    {
        public MetaData MetaData { get; set; }
        public IDataTypeDefinition[] DataTypesInstalled { get; set; }
        public ILanguage[] LanguagesInstalled { get; set; }
        public IDictionaryItem[] DictionaryItemsInstalled { get; set; }
        public IMacro[] MacrosInstalled { get; set; }
        public string[] FilesInstalled { get; set; }
        public ITemplate[] TemplatesInstalled { get; set; }
        public IContentType[] ContentTypesInstalled { get; set; }
        public IFile[] StylesheetsInstalled { get; set; }
        public IContent[] ContentInstalled { get; set; }
        public PackageAction[] Actions { get; set; }
        public bool PackageInstalled { get; set; }
    }


    internal static class InstallationSummaryExtentions
    {
        public static InstallationSummary InitEmpty(this InstallationSummary @this)
        {
            @this.Actions = new PackageAction[0];
            @this.ContentInstalled = new IContent[0];
            @this.ContentTypesInstalled = new IContentType[0];
            @this.DataTypesInstalled = new IDataTypeDefinition[0];
            @this.DictionaryItemsInstalled = new IDictionaryItem[0];
            @this.FilesInstalled = new string[0];
            @this.LanguagesInstalled = new ILanguage[0];
            @this.MacrosInstalled = new IMacro[0];
            @this.MetaData = new MetaData();
            @this.TemplatesInstalled = new ITemplate[0];
            @this.PackageInstalled = false;
            return @this;
        }
    }
}