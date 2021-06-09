using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Packaging;

namespace Umbraco.Cms.Core.Packaging
{
    [DataContract(Name = "packageInstance")]
    public class PackageDefinition : IPackageInfo
    {
        /// <summary>
        /// Converts a <see cref="CompiledPackage"/> model to a <see cref="PackageDefinition"/> model
        /// </summary>
        /// <param name="compiled"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is used only for conversions and will not 'get' a PackageDefinition from the repository with a valid ID
        /// </remarks>
        public static PackageDefinition FromCompiledPackage(CompiledPackage compiled)
            => new PackageDefinition
            {
                Name = compiled.Name
            };

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "packageGuid")]
        public Guid PackageId { get; set; }

        [DataMember(Name = "name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The full path to the package's zip file when it was installed (or is being installed)
        /// </summary>
        [ReadOnly(true)]
        [DataMember(Name = "packagePath")]
        public string PackagePath { get; set; } = string.Empty;

        [DataMember(Name = "contentLoadChildNodes")]
        public bool ContentLoadChildNodes { get; set; }

        [DataMember(Name = "contentNodeId")]
        public string ContentNodeId { get; set; } = string.Empty;

        [DataMember(Name = "macros")]
        public IList<string> Macros { get; set; } = new List<string>();

        [DataMember(Name = "languages")]
        public IList<string> Languages { get; set; } = new List<string>();

        [DataMember(Name = "dictionaryItems")]
        public IList<string> DictionaryItems { get; set; } = new List<string>();

        [DataMember(Name = "templates")]
        public IList<string> Templates { get; set; } = new List<string>();

        [DataMember(Name = "documentTypes")]
        public IList<string> DocumentTypes { get; set; } = new List<string>();

        [DataMember(Name = "mediaTypes")]
        public IList<string> MediaTypes { get; set; } = new List<string>();

        [DataMember(Name = "stylesheets")]
        public IList<string> Stylesheets { get; set; } = new List<string>();

        [DataMember(Name = "dataTypes")]
        public IList<string> DataTypes { get; set; } = new List<string>();

        [DataMember(Name = "mediaUdis")]
        public IList<GuidUdi> MediaUdis { get; set; } = Array.Empty<GuidUdi>();

        [DataMember(Name = "mediaLoadChildNodes")]
        public bool MediaLoadChildNodes { get; set; }


    }

}
