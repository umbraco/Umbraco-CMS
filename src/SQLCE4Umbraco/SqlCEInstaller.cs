/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Resources;
using SQLCE4Umbraco;
using umbraco.DataLayer.Utility.Installer;
using System.Diagnostics;

namespace SqlCE4Umbraco
{
    /// <summary>
    /// Database installer for an SQL Server data source.
    /// </summary>
    public class SqlCEInstaller : DefaultInstallerUtility<SqlCEHelper>
    {
        #region Private Constants
       
        /// <summary>The latest database version this installer supports.</summary>
        private const DatabaseVersion LatestVersionSupported = DatabaseVersion.Version4_8;

        /// <summary>The specifications to determine the database version.</summary>
        private static readonly VersionSpecs[] m_VersionSpecs = new VersionSpecs[] {
					new VersionSpecs("CONSTRAINT_NAME","INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS", "FK_umbracoUser2app_umbracoApp", false, DatabaseVersion.Version4_8), 
					new VersionSpecs("id","umbracoNode", "-21", DatabaseVersion.Version4_1),        
					new VersionSpecs("action","umbracoAppTree",DatabaseVersion.Version4),
                    new VersionSpecs("description","cmsContentType",DatabaseVersion.Version3),
                    new VersionSpecs("id","sysobjects",DatabaseVersion.None) };

        #endregion

        #region Public Properties

        /// <summary>
        /// This ensures that the database exists, then runs the base method
        /// </summary>
        public override bool CanConnect
        {
            get
            {                
                SqlHelper.CreateEmptyDatabase();
                return base.CanConnect;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the installer can upgrade the data source.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the installer can upgrade the data source; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Empty data sources can't be upgraded, just installed.</remarks>
        public override bool CanUpgrade
        {
            get
            {
                return CurrentVersion == DatabaseVersion.Version4_1;
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the version specification for evaluation by DetermineCurrentVersion.
        /// Only first matching specification is taken into account.
        /// </summary>
        /// <value>The version specifications.</value>
        protected override VersionSpecs[] VersionSpecs
        {
            get { return m_VersionSpecs; }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCEInstaller"/> class.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        public SqlCEInstaller(SqlCEHelper sqlHelper) : base(sqlHelper, LatestVersionSupported)
        { }

        #endregion

        #region DefaultInstaller Members       

		/// <summary>
		/// Returns the sql to do a full install
		/// </summary>
		protected override string FullInstallSql
		{
			get { return SqlCEResources.Total; }
		}


		/// <summary>
		/// Returns the sql to do an upgrade
		/// </summary>
		protected override string UpgradeSql
		{
			get
			{
				string upgradeFile = string.Format("{0}_Upgrade", CurrentVersion.ToString());
				return SqlCEResources.ResourceManager.GetString(upgradeFile);
			}
		}

        // We need to override this as the default way of detection a db connection checks for systables that doesn't exist
        // in a CE db
        protected override DatabaseVersion DetermineCurrentVersion()
        {
            DatabaseVersion version = base.DetermineCurrentVersion();
            if (version != DatabaseVersion.Unavailable)
            {
                return version;
            }

            // verify connection
            try
            {
                if (SqlCeApplicationBlock.VerifyConnection(base.SqlHelper.ConnectionString))
                    return DatabaseVersion.None;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }

            return DatabaseVersion.Unavailable;
        }

        #endregion
    }
}
