using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Core.Packaging.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    internal class InstallationSummary
    {
         public MetaData MetaData { get; set; }
         public IEnumerable<int> DataTypesInstalled { get; set; }
         public IEnumerable<int> LanguagesInstalled { get; set; }
         public IEnumerable<int> DictionaryItemsInstalled { get; set; }
         public IEnumerable<int> MacrosInstalled { get; set; }
         public IEnumerable<KeyValuePair<string, bool>> FilesInstalled { get; set;}
         public IEnumerable<int> TemplatesInstalled { get; set; }
         public IEnumerable<int> DocumentTypesInstalled { get; set; }
         public IEnumerable<int> StylesheetsInstalled { get; set; }
         public IEnumerable<int> DocumentsInstalled { get; set; }
         public IEnumerable<InstallAction> InstallActions { get; set; }
         public IEnumerable<UninstallAction> UninstallActions { get; set; }
    }
}