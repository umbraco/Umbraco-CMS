using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Models
{
    /// <summary>
    /// A model that represents uploading a local package
    /// </summary>
    [DataContract(Name = "localPackageInstallModel")]
    public class LocalPackageInstallModel : PackageInstallModel, INotificationModel
    {
        [DataMember(Name = "notifications")]
        public List<BackOfficeNotification> Notifications { get; } = new List<BackOfficeNotification>();

        /// <summary>
        /// A flag to determine if this package is compatible to be installed
        /// </summary>
        [DataMember(Name = "isCompatible")]
        public bool IsCompatible { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }


        [DataMember(Name = "containsUnsecureFiles")]
        public bool ContainsUnsecureFiles { get; set; }

        [DataMember(Name = "containsTemplateConflicts")]
        public bool ContainsTemplateConflicts => ConflictingTemplateAliases != null && ConflictingTemplateAliases.Count > 0;

        [DataMember(Name = "containsStyleSheetConflicts")]
        public bool ContainsStyleSheetConflicts => ConflictingStyleSheetNames != null && ConflictingStyleSheetNames.Count > 0;

        [DataMember(Name = "containsMacroConflict")]
        public bool ContainsMacroConflict => ConflictingMacroAliases != null && ConflictingMacroAliases.Count > 0;

        /// <summary>
        /// Key value of name + alias
        /// </summary>
        [DataMember(Name = "conflictingTemplateAliases")]
        public IDictionary<string, string> ConflictingTemplateAliases { get; set; }

        /// <summary>
        /// Key value of name + alias
        /// </summary>
        [DataMember(Name = "conflictingStyleSheetNames")]
        public IDictionary<string, string> ConflictingStyleSheetNames { get; set; }

        /// <summary>
        /// Key value of name + alias
        /// </summary>
        [DataMember(Name = "conflictingMacroAliases")]
        public IDictionary<string, string> ConflictingMacroAliases { get; set; }


    }
}
