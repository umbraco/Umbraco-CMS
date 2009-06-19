using System;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using umbraco.DataLayer;
using umbraco.DataLayer.Utility.Installer;

namespace umbraco.presentation.install.steps
{
    /// <summary>
    ///	Database detection step in the installer wizard.
    /// </summary>
    public partial class detect : System.Web.UI.UserControl
    {
        /// <summary>The installer associated with the chosen connection string.</summary>
        private IInstallerUtility m_Installer;

        /// <summary>
        /// The installer associated with the chosen connection string.
        /// Will be initialized if <c>m_Installer</c> is <c>null</c>.
        /// </summary>
        protected IInstallerUtility Installer
        {
            get
            {
                if (m_Installer == null)
                    m_Installer = SqlHelper.Utility.CreateInstaller();
                return m_Installer;
            }
        }

        /// <summary>Returns the current SQLHelper.</summary>
        protected static ISqlHelper SqlHelper
        {
            get { return BusinessLogic.Application.SqlHelper; }
        }

        /// <summary>
        /// Returns whether the selected database is an embedded database.
        /// </summary>
        protected bool IsEmbeddedDatabase
        {
            get { return DatabaseType.SelectedItem.Text.Contains("VistaDB"); }
        }

        /// <summary>
        /// Returns whether the connection string is set by direct text input.
        /// </summary>
        protected bool ManualConnectionString
        {
            get { return DatabaseType.SelectedItem.Value.Length == 0; }
        }

        /// <summary>
        /// Shows the right panel to the user.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Disable back/forward buttons
            Page.FindControl("next").Visible = false;

            // Does the user have to enter a connection string?
            if (settings.Visible)
                ShowDatabaseSettings();
            else
                ShowDatabaseInstallation();
        }

        /// <summary>
        /// Prepares and shows the database settings panel.
        /// </summary>
        protected void ShowDatabaseSettings()
        {
            // Did the connection string of web.config get loaded?
            if (!Page.IsPostBack)
            {
                // Parse the connection string
                DbConnectionStringBuilder connectionStringBuilder = new DbConnectionStringBuilder();
                connectionStringBuilder.ConnectionString = GlobalSettings.DbDSN;

                // Prepare the fields

                // Prepare data layer type
                string datalayerType = GetConnectionStringValue(connectionStringBuilder, "datalayer");
                if (datalayerType.Length > 0)
                {
                    foreach (ListItem item in DatabaseType.Items)
                        if (item.Value != String.Empty && ((string)datalayerType).Contains(item.Value))
                            DatabaseType.SelectedValue = item.Value;
                }
                DatabaseType_SelectedIndexChanged(this, new EventArgs());

                // Prepare other fields
                DatabaseServer.Text = GetConnectionStringValue(connectionStringBuilder, "server");
                DatabaseName.Text = GetConnectionStringValue(connectionStringBuilder, "database");
                DatabaseUsername.Text = GetConnectionStringValue(connectionStringBuilder, "user id");
                DatabasePassword.Text = GetConnectionStringValue(connectionStringBuilder, "password");
            }

            // Make sure ASP.Net displays the password text
            DatabasePassword.Attributes["value"] = DatabasePassword.Text;
        }

        /// <summary>
        /// Shows the installation/upgrade panel.
        /// </summary>
        protected void ShowDatabaseInstallation()
        {
            identify.Visible = true;

            if (Installer.CurrentVersion == DatabaseVersion.None)
            {
                dbEmpty.Visible = true;
                dbUpgrade.Visible = false;
            }
            else
            {
                version.Text = Installer.CurrentVersion.ToString();
            }
            if (Installer.IsLatestVersion)
            {

                settings.Visible = false;
                installed.Visible = true;
                // Enable back/forward buttons
                Page.FindControl("next").Visible = true;

            }
            else
            {
                if (Installer.CanUpgrade)
                {
                    upgrade.Visible = true;
                    other.Visible = true;
                }
                else if (Installer.IsEmpty)
                {
                    install.Visible = true;
                    none.Visible = true;
                }
                else
                {
                    retry.Visible = true;
                    error.Visible = true;
                }
            }
        }

        /// <summary>
        /// Tries to connect to the database and saves the new connection string if successful.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        protected void DatabaseConnectButton_Click(object sender, EventArgs e)
        {
            // Build the new connection string
            DbConnectionStringBuilder connectionStringBuilder = CreateConnectionString();

            // Try to connect to the database
            Exception error = null;
            try
            {
                ISqlHelper sqlHelper = DataLayerHelper.CreateSqlHelper(connectionStringBuilder.ConnectionString);
                m_Installer = sqlHelper.Utility.CreateInstaller();
                if (!Installer.CanConnect)
                    throw new Exception("The installer cannot connect to the database.");
            }
            catch (Exception ex)
            {
                error = new Exception("Database connection initialisation failed.", ex);
            }

            // Save the new connection string
            if (error == null)
            {
                try
                {
                    GlobalSettings.DbDSN = connectionStringBuilder.ConnectionString;
                }
                catch (Exception ex)
                {
                    error = new Exception("Could not save the web.config file. Please modify the connection string manually.", ex);
                }
            }

            // Show database installation panel or error message if not successful
            if (error == null)
            {
                settings.Visible = false;
                ShowDatabaseInstallation();
            }
            else
            {
                DatabaseError.InnerText = String.Format("{0} {1}", error.Message, error.InnerException.Message);
            }
        }

        /// <summary>
        /// Creates the connection string with the values the user has supplied.
        /// </summary>
        /// <returns></returns>
        protected DbConnectionStringBuilder CreateConnectionString()
        {
            DbConnectionStringBuilder connectionStringBuilder = new DbConnectionStringBuilder();

            if (ManualConnectionString)
            {
                connectionStringBuilder.ConnectionString = ConnectionString.Text;
            }
            else if (!IsEmbeddedDatabase)
            {
                connectionStringBuilder["server"] = DatabaseServer.Text;
                connectionStringBuilder["database"] = DatabaseName.Text;
                connectionStringBuilder["user id"] = DatabaseUsername.Text;
                connectionStringBuilder["password"] = DatabasePassword.Text;
            }

            if (!String.IsNullOrEmpty(DatabaseType.SelectedValue) && DatabaseType.SelectedValue!="SqlServer")
                connectionStringBuilder["datalayer"] = DatabaseType.SelectedValue;

            return connectionStringBuilder;
        }

        /// <summary>
        /// Gets the value of the specified item in the connection string.
        /// </summary>
        /// <param name="connectionStringBuilder">The connection string.</param>
        /// <param name="keyword">Name of the item.</param>
        /// <returns>The value of the item, or an empty string if not found.</returns>
        protected string GetConnectionStringValue(DbConnectionStringBuilder connectionStringBuilder, string keyword)
        {
            object value = null;
            connectionStringBuilder.TryGetValue(keyword, out value);
            return (string)value ?? String.Empty;
        }

        /// <summary>
        /// Show the needed fields according to the database type.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        protected void DatabaseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            DatabaseServerItem.Visible = !ManualConnectionString && !IsEmbeddedDatabase;
            DatabaseUsernameItem.Visible = !ManualConnectionString && !IsEmbeddedDatabase;
            DatabasePasswordItem.Visible = !ManualConnectionString && !IsEmbeddedDatabase;
            DatabaseNameItem.Visible = !ManualConnectionString && !IsEmbeddedDatabase;
            
            DatabaseConnectionString.Visible = ManualConnectionString;
        }

        protected void upgrade_Click(object sender, System.EventArgs e)
        {
            Installer.Install();

            Response.Redirect("default.aspx?installStep=defaultUser", true);

            ((HtmlInputHidden)Page.FindControl("step")).Value = "upgradeIndex";
            ((Button)Page.FindControl("next")).Visible = true;

            settings.Visible = false;
            upgrade.Visible = false;
            identify.Visible = false;
            confirms.Visible = true;
            upgradeConfirm.Visible = true;
        }

        protected void install_Click(object sender, System.EventArgs e)
        {
            Installer.Install();

            ((HtmlInputHidden)Page.FindControl("step")).Value = "validatePermissions";
            ((Button)Page.FindControl("next")).Visible = true;

            settings.Visible = false;
            install.Visible = false;
            identify.Visible = false;
            confirms.Visible = true;
            installConfirm.Visible = true;

            //after the database install we will login the default admin user so we can install boost and nitros later on

            if (GlobalSettings.ConfigurationStatus.Trim() == "")
                BasePages.UmbracoEnsuredPage.doLogin(new global::umbraco.BusinessLogic.User(0));
        }
    }
}