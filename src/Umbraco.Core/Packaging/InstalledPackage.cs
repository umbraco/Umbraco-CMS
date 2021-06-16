using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Packaging
{
    [DataContract(Name = "installedPackage")]
    public class InstalledPackage
    {
        [DataMember(Name = "name", IsRequired = true)]
        [Required]
        public string PackageName { get; set; }

        // TODO: Version? Icon? Other metadata?

        [DataMember(Name = "packageView")]
        public string PackageView { get; set; }

        [DataMember(Name = "plans")]
        public IEnumerable<InstalledPackageMigrationPlans> PackageMigrationPlans { get; set; } = Enumerable.Empty<InstalledPackageMigrationPlans>();

        [DataMember(Name = "hasPendingMigrations")]
        public bool HasPendingMigrations => PackageMigrationPlans.Any(x => x.HasPendingMigrations);
    }

}
