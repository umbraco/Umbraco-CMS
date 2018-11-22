using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// A model that represents uploading a local package
    /// </summary>
    [DataContract(Name = "localPackageInstallModel")]
    public class LocalPackageInstallModel : PackageInstallModel, IHaveUploadedFiles, INotificationModel
    {
        public LocalPackageInstallModel()
        {
            UploadedFiles = new List<ContentItemFile>();
            Notifications = new List<Notification>();
        }
        
        public List<ContentItemFile> UploadedFiles { get; private set; }
        
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }

        /// <summary>
        /// A flag to determine if this package is compatible to be installed
        /// </summary>
        [DataMember(Name = "isCompatible")]
        public bool IsCompatible { get; set; }

        /// <summary>
        /// The minimum umbraco version that this package is pinned to
        /// </summary>
        [DataMember(Name = "umbracoVersion")]
        public string UmbracoVersion { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "containsUnsecureFiles")]
        public bool ContainsUnsecureFiles { get; set; }

        [DataMember(Name = "containsTemplateConflicts")]
        public bool ContainsTemplateConflicts { get; set; }

        [DataMember(Name = "containsStyleSheetConflicts")]
        public bool ContainsStyleSheetConflicts { get; set; }

        [DataMember(Name = "containsMacroConflict")]
        public bool ContainsMacroConflict { get; set; }

        [DataMember(Name = "containsLegacyPropertyEditors")]
        public bool ContainsLegacyPropertyEditors { get; set; }

        [DataMember(Name = "containsBinaryFileErrors")]
        public bool ContainsBinaryFileErrors { get; set; }

        [DataMember(Name = "conflictingTemplateAliases")]
        public IDictionary<string, string> ConflictingTemplateAliases { get; set; }

        [DataMember(Name = "conflictingStyleSheetNames")]
        public IDictionary<string, string> ConflictingStyleSheetNames { get; set; }

        [DataMember(Name = "conflictingMacroAliases")]
        public IDictionary<string, string> ConflictingMacroAliases { get; set; }

        [DataMember(Name = "readMe")]
        public string ReadMe { get; set; }

        [DataMember(Name = "licenseUrl")]
        public string LicenseUrl { get; set; }

        [DataMember(Name = "license")]
        public string License { get; set; }

        [DataMember(Name = "authorUrl")]
        public string AuthorUrl { get; set; }

        [DataMember(Name = "author")]
        public string Author { get; set; }

        [DataMember(Name = "iconUrl")]
        public string IconUrl { get; set; }
    }
}