using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.Membership;

namespace umbraco.cms.businesslogic.packager
{
    [Obsolete("This class is not used and will be removed in future versions")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Package
    {
        public Package()
        {
        }

        /// <summary>
        /// Initialize package install status object by specifying the internal id of the installation.
        /// The id is specific to the local umbraco installation and cannot be used to identify the package in general.
        /// Use the Package(Guid) constructor to check whether a package has been installed
        /// </summary>
        /// <param name="Id">The internal id.</param>
        public Package(int Id)
        {
            Initialize(Id);
        }

        public Package(Guid Id)
        {
            using (var scope = Current.ScopeProvider.CreateScope())
            {
                int installStatusId = scope.Database.ExecuteScalar<int>(
                    "select id from umbracoInstalledPackages where package = @package and upgradeId = 0", new { package = Id});

                if (installStatusId > 0)
                    Initialize(installStatusId);
                else
                    throw new ArgumentException("Package with id '" + Id.ToString() + "' is not installed");

                scope.Complete();
            }
        }

        private void Initialize(int id)
        {
            using (var scope = Current.ScopeProvider.CreateScope())
            {
                var f = scope.Database.Fetch<dynamic>(
                    "select id, uninstalled, upgradeId, installDate, userId, package, versionMajor, versionMinor, versionPatch from umbracoInstalledPackages where id = @id",
                    new { id }).FirstOrDefault();
                if (f != null)
                {
                    Id = id;
                    Uninstalled = f.uninstalled;
                    UpgradeId = f.upgradeId;
                    InstallDate = f.installDate;
                    User = Current.Services.UserService.GetUserById(f.userId);
                    PackageId = f.package;
                    VersionMajor = f.versionMajor;
                    VersionMinor = f.versionMinor;
                    VersionPatch = f.versionPatch;
                }

                scope.Complete();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)] // ;-((
        public void Save()
        {
            using (var scope = Current.ScopeProvider.CreateScope())
            {
                // check if package status exists
                if (Id == 0)
                {
                    // The method is synchronized
                    scope.Database.Execute("INSERT INTO umbracoInstalledPackages (uninstalled, upgradeId, installDate, userId, versionMajor, versionMinor, versionPatch) VALUES (@uninstalled, @upgradeId, @installDate, @userId, @versionMajor, @versionMinor, @versionPatch)",
                        new
                        {
                            uninstalled = Uninstalled,
                            upgradeId = UpgradeId,
                            installData = InstallDate,
                            userId = User.Id,
                            versionMajor = VersionMajor,
                            versionMinor = VersionMinor,
                            versionPath = VersionPatch,
                            id = Id
                        });
                    Id = scope.Database.ExecuteScalar<int>("SELECT MAX(id) FROM umbracoInstalledPackages");
                }

                scope.Database.Execute(
                    "update umbracoInstalledPackages set " +
                    "uninstalled = @uninstalled, " +
                    "upgradeId = @upgradeId, " +
                    "installDate = @installDate, " +
                    "userId = @userId, " +
                    "versionMajor = @versionMajor, " +
                    "versionMinor = @versionMinor, " +
                    "versionPatch = @versionPatch " +
                    "where id = @id",
                    new
                    {
                        uninstalled = Uninstalled,
                        upgradeId = UpgradeId,
                        installData = InstallDate,
                        userId = User.Id,
                        versionMajor = VersionMajor,
                        versionMinor = VersionMinor,
                        versionPath = VersionPatch,
                        id = Id
                    });

                scope.Complete();
            }
        }

        public bool Uninstalled { get; set; }


        public IUser User { get; set; }


        public DateTime InstallDate { get; set; }


        public int Id { get; set; }


        public int UpgradeId { get; set; }


        public Guid PackageId { get; set; }


        public int VersionPatch { get; set; }


        public int VersionMinor { get; set; }


        public int VersionMajor { get; set; }
    }
}
