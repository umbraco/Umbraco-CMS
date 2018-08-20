using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace umbraco.cms.businesslogic.packager
{
    [Obsolete("This class is not used and will be removed in future versions")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Package
    {
        /// <summary>
        /// Unused, please do not use
        /// </summary>
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

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
            using (var sqlHelper = Application.SqlHelper)
            {
                int installStatusId = sqlHelper.ExecuteScalar<int>(
                    "select id from umbracoInstalledPackages where package = @package and upgradeId = 0",
                    sqlHelper.CreateParameter("@package", Id));

                if (installStatusId > 0)
                    Initialize(installStatusId);
                else
                    throw new ArgumentException("Package with id '" + Id.ToString() + "' is not installed");
            }
        }

        private void Initialize(int id)
        {
            using (var sqlHelper = Application.SqlHelper) {
                using (IRecordsReader dr =
                 sqlHelper.ExecuteReader(
                     "select id, uninstalled, upgradeId, installDate, userId, package, versionMajor, versionMinor, versionPatch from umbracoInstalledPackages where id = @id",
                     sqlHelper.CreateParameter("@id", id)))
                {
                    if (dr.Read())
                    {
                        Id = id;
                        Uninstalled = dr.GetBoolean("uninstalled");
                        UpgradeId = dr.GetInt("upgradeId");
                        InstallDate = dr.GetDateTime("installDate");
                        User = User.GetUser(dr.GetInt("userId"));
                        PackageId = dr.GetGuid("package");
                        VersionMajor = dr.GetInt("versionMajor");
                        VersionMinor = dr.GetInt("versionMinor");
                        VersionPatch = dr.GetInt("versionPatch");
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Save()
        {
            using (var sqlHelper = Application.SqlHelper)
            {
                IParameter[] values = {
                    sqlHelper.CreateParameter("@uninstalled", Uninstalled),
                    sqlHelper.CreateParameter("@upgradeId", UpgradeId),
                    sqlHelper.CreateParameter("@installDate", InstallDate),
                    sqlHelper.CreateParameter("@userId", User.Id),
                    sqlHelper.CreateParameter("@versionMajor", VersionMajor),
                    sqlHelper.CreateParameter("@versionMinor", VersionMinor),
                    sqlHelper.CreateParameter("@versionPatch", VersionPatch),
                    sqlHelper.CreateParameter("@id", Id)
                };

                // check if package status exists
                if (Id == 0)
                {
                    // The method is synchronized
                    sqlHelper.ExecuteNonQuery("INSERT INTO umbracoInstalledPackages (uninstalled, upgradeId, installDate, userId, versionMajor, versionMinor, versionPatch) VALUES (@uninstalled, @upgradeId, @installDate, @userId, @versionMajor, @versionMinor, @versionPatch)", values);
                    Id = sqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM umbracoInstalledPackages");
                }

                sqlHelper.ExecuteNonQuery(
                    "update umbracoInstalledPackages set " +
                    "uninstalled = @uninstalled, " +
                    "upgradeId = @upgradeId, " +
                    "installDate = @installDate, " +
                    "userId = @userId, " +
                    "versionMajor = @versionMajor, " +
                    "versionMinor = @versionMinor, " +
                    "versionPatch = @versionPatch " +
                    "where id = @id",
                    values);
            }
        }

        public bool Uninstalled { get; set; }


        public User User { get; set; }


        public DateTime InstallDate { get; set; }


        public int Id { get; set; }


        public int UpgradeId { get; set; }


        public Guid PackageId { get; set; }


        public int VersionPatch { get; set; }


        public int VersionMinor { get; set; }


        public int VersionMajor { get; set; }
    }
}