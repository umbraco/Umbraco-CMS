/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Text.RegularExpressions;

namespace umbraco.DataLayer.Utility.Installer
{

    /// <summary>
    /// Base class for installers that use an ISqlHelper as data source.
    /// </summary>
    /// <typeparam name="S">The SQL helper type.</typeparam>
    [Obsolete("The legacy installers are no longer used and will be removed from the codebase in the future")]
    public abstract class DefaultInstallerUtility<S> : BaseUtility<S>, IInstallerUtility where S : ISqlHelper
    {
        #region Private Fields

        /// <summary>The latest available version this installer provides.</summary>
        private readonly DatabaseVersion m_LatestVersion;

        /// <summary>The currently installed database version, <c>null</c> if undetermined.</summary>
        private DatabaseVersion? m_CurrentVersion = null;

        /// <summary>Regular expression that finds multiline block comments.</summary>
        private static readonly Regex m_findComments = new Regex(@"\/\*.*?\*\/", RegexOptions.Singleline | RegexOptions.Compiled);

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the current data source version.
        /// </summary>
        /// <value>The current version.</value>
        public DatabaseVersion CurrentVersion
        {
            get
            {
                if (!m_CurrentVersion.HasValue)
                    m_CurrentVersion = DetermineCurrentVersion();
                return m_CurrentVersion.Value;
            }
        }

        /// <summary>
        /// Gets the latest available version.
        /// </summary>
        /// <value>The latest version.</value>
        public DatabaseVersion LatestVersion
        {
            get { return m_LatestVersion; }
        }

        /// <summary>
        /// Gets a value indicating whether this installer can connect to the data source.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the installer can connect; otherwise, <c>false</c>.
        /// </value>
        public virtual bool CanConnect
        {
            get { return CurrentVersion != DatabaseVersion.Unavailable; }
        }

        /// <summary>
        /// Gets a value indicating whether the data source is empty and ready for installation.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the data source is empty; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsEmpty
        {
            get { return CurrentVersion == DatabaseVersion.None; }
        }
		        
        /// <summary>
        /// Gets a value indicating whether the data source has an up to date version.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the data source is up to date; otherwise, <c>false</c>.
        /// </value>
        public bool IsLatestVersion
        {
            get { return CurrentVersion == LatestVersion; }
        }

        /// <summary>
        /// Gets a value indicating whether the installer can upgrade the data source.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the installer can upgrade the data source; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Empty data sources can't be upgraded, just installed.</remarks>
        public virtual bool CanUpgrade
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the version specification for evaluation by DetermineCurrentVersion.
        /// Only first matching specification is taken into account.
        /// </summary>
        /// <value>The version specifications.</value>
		protected abstract VersionSpecs[] VersionSpecs { get; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultInstallerUtility&lt;S&gt;"/> class.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        public DefaultInstallerUtility(S sqlHelper)
            : this(sqlHelper, DatabaseVersion.Unavailable)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultInstallerUtility&lt;S&gt;"/> class.
        /// </summary>
        /// <param name="sqlHelper">The SQL helper.</param>
        /// <param name="latestVersion">The latest available version.</param>
        public DefaultInstallerUtility(S sqlHelper, DatabaseVersion latestVersion)
            : base(sqlHelper)
        {
            m_LatestVersion = latestVersion;
        }

        #endregion

		#region Protected Properties

		protected abstract string FullInstallSql { get; }
		protected abstract string UpgradeSql { get; }

		#endregion

        #region IUmbracoInstaller Members

        /// <summary>
        /// Installs the latest version into the data source.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        /// If installing or upgrading is not supported.</exception>
		public void Install()
		{
			if (IsLatestVersion)
				return;

			// installation on empty database
			if (IsEmpty)
			{
				NewInstall(FullInstallSql);
			}
			else
			// upgrade from older version
			{
				if (!CanUpgrade)
					throw new NotSupportedException("Upgrading from this version is not supported.");
				
				// execute version specific upgrade set
				Upgrade(UpgradeSql);
			}
		}

        #endregion

        #region Protected Methods
		
		protected virtual void NewInstall(string sql)
		{
			ExecuteStatements(sql);
		}

		protected virtual void Upgrade(string sql)
		{
			ExecuteStatements(sql);
		}

        /// <summary>
        /// Determines the current version of the SQL data source,
        /// by attempting to select a certain field from a certain table.
        /// The specifications are retrieved using the VersionSpecs property.
        /// </summary>
        /// <returns>The current version of the SQL data source</returns>
        protected virtual DatabaseVersion DetermineCurrentVersion()
        {
            foreach (VersionSpecs v in VersionSpecs)
            {
                try
                {
                    if(v.ExpectedRows > -1)
                    {
                        using (var reader = SqlHelper.ExecuteReader(v.Sql))
                        {
                            var rowCount = 0;
                            while (reader.Read())
                                rowCount++;

                            if (v.ExpectedRows != rowCount)
                                continue;
                        }
                    }
                    else
                    {
                        SqlHelper.ExecuteNonQuery(v.Sql);
                    }

                    //if (!String.IsNullOrEmpty(v.Table) && !String.IsNullOrEmpty(v.Field) && !String.IsNullOrEmpty(v.Value))
                    //{
                    //    IRecordsReader reader = SqlHelper.ExecuteReader(string.Format("SELECT {0} FROM {1} WHERE {0}={2}", v.Field, v.Table, v.Value));
                    //    var canRead = reader.Read();
                    //    if ((v.ShouldExist && !canRead) || (!v.ShouldExist && canRead))
                    //        continue;
                    //}
                    //else if (String.IsNullOrEmpty(v.Table))
                    //    SqlHelper.ExecuteNonQuery(string.Format("SELECT {0}", v.Field));
                    //else
                    //    SqlHelper.ExecuteNonQuery(string.Format("SELECT {0} FROM {1}", v.Field, v.Table));

                    return v.Version;
                }
                catch { }
            }

            return DatabaseVersion.Unavailable;
        }

        /// <summary>
        /// Executes a list of semicolon separated statements.
        /// Statements starting with
        /// </summary>
        /// <param name="statements">The statements.</param>
        protected void ExecuteStatements(string statements)
        {
            if (string.IsNullOrEmpty(statements))
            {
                throw new ArgumentNullException("statements", "The sql statement to execute is empty. Database version: " + CurrentVersion.ToString());
            }

            // replace block comments by whitespace
            statements = m_findComments.Replace(statements, " ");
            // execute all non-empty statements
            foreach (string statement in statements.Split(";".ToCharArray()))
            {
                string rawStatement = statement.Trim();
                if (rawStatement.Length > 0)
                    SqlHelper.ExecuteNonQuery(rawStatement);
            }
        }

        #endregion
    }

   
}
