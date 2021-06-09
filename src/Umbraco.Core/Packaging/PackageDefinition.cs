using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Packaging
{
    // This is the thing that goes in the createdPackages.config
    [DataContract(Name = "packageInstance")]
    public class PackageDefinition
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "packageGuid")]
        public Guid PackageId { get; set; }

        [DataMember(Name = "name")]
        [Required]
        public string Name { get; set; } = string.Empty;

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
