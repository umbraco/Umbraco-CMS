using System;
using System.Data.Common;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using umbraco.DataLayer;
using umbraco.DataLayer.Utility.Installer;
using System.IO;
using umbraco.IO;

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
            get { return Request["database"] == "embedded" || GlobalSettings.DbDSN.ToLower().Contains("SQLCE4Umbraco.SqlCEHelper".ToLower()); }
        }

        protected bool IsConfigured
        {
            get { return DatabaseType.SelectedValue != ""; }
        }

        /// <summary>
        /// Returns whether the selected database is an embedded database.
        /// </summary>
        protected bool HasEmbeddedDatabaseFiles
        {
            get {
                // check if sql ce is present
                    if (
                        !File.Exists(IOHelper.MapPath(Path.Combine(IOHelper.ResolveUrl(SystemDirectories.Bin), "System.Data.SqlServerCe.dll"))) ||
                        !File.Exists(IOHelper.MapPath(Path.Combine(IOHelper.ResolveUrl(SystemDirectories.Bin), "SQLCE4Umbraco.dll")))
                        )
                        return false;
                    else
                        return true;
            }
        }

        /// <summary>
        /// Returns whether the connection string is set by direct text input.
        /// </summary>
        protected bool ManualConnectionString
        {
            get { return Request["database"] == "advanced"; }
        }

        /// <summary>
        /// Shows the right panel to the user.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Does the user have to enter a connection string?
            if (settings.Visible && !Page.IsPostBack)
                ShowDatabaseSettings();
            
        }

        /// <summary>
        /// Prepares and shows the database settings panel.
        /// </summary>
        protected void ShowDatabaseSettings()
        {
                // Parse the connection string
                DbConnectionStringBuilder connectionStringBuilder = new DbConnectionStringBuilder();
                connectionStringBuilder.ConnectionString = GlobalSettings.DbDSN;

                // "Data Source=.\\SQLEXPRESS;Initial Catalog=BB_Umbraco_Sandbox1;integrated security=false;user id=umbraco;pwd=umbraco"

                // Prepare the fields
                string database = GetConnectionStringValue(connectionStringBuilder, "database");
                string server = GetConnectionStringValue(connectionStringBuilder, "server"); 

                // Prepare data layer type
                string datalayerType = GetConnectionStringValue(connectionStringBuilder, "datalayer");
                if (datalayerType.Length > 0)
                {
                    foreach (ListItem item in DatabaseType.Items)
                        if (item.Value != String.Empty && ((string)datalayerType).Contains(item.Value))
                            DatabaseType.SelectedValue = item.Value;
                }
                else if (GlobalSettings.DbDSN != "server=.\\SQLEXPRESS;database=DATABASE;user id=USER;password=PASS")
                    DatabaseType.SelectedValue = "SqlServer";
                
                DatabaseType_SelectedIndexChanged(this, new EventArgs());

                // Prepare other fields
                DatabaseServer.Text = GetConnectionStringValue(connectionStringBuilder, "server");
                if (string.IsNullOrEmpty(DatabaseServer.Text)) DatabaseServer.Text = GetConnectionStringValue(connectionStringBuilder, "Data Source");
                DatabaseName.Text = GetConnectionStringValue(connectionStringBuilder, "database");
                if (string.IsNullOrEmpty(DatabaseName.Text)) DatabaseName.Text = GetConnectionStringValue(connectionStringBuilder, "Initial Catalog");
                DatabaseUsername.Text = GetConnectionStringValue(connectionStringBuilder, "user id");
                DatabasePassword.Text = GetConnectionStringValue(connectionStringBuilder, "password");
                if (string.IsNullOrEmpty(DatabasePassword.Text)) DatabasePassword.Text = GetConnectionStringValue(connectionStringBuilder, "pwd");

                toggleVisible(DatabaseServerItem, !ManualConnectionString && !IsEmbeddedDatabase);
                toggleVisible(DatabaseUsernameItem, !ManualConnectionString && !IsEmbeddedDatabase);
                toggleVisible(DatabasePasswordItem, !ManualConnectionString && !IsEmbeddedDatabase);
                toggleVisible(DatabaseNameItem, !ManualConnectionString && !IsEmbeddedDatabase);


                if (IsEmbeddedDatabase)
                    dbinit.Text = "$('#databaseOptionEmbedded').click();$('#databaseOptionEmbedded').change();";
                else if(ManualConnectionString)
                    dbinit.Text = "$('#databaseOptionAdvanced').click();$('#databaseOptionAdvanced').change();";
                else if(DatabaseType.SelectedValue == "SqlServer")
                    dbinit.Text = "$('#databaseOptionBlank').click();$('#databaseOptionBlank').change();";
                else if (DatabaseType.SelectedValue == "SqlAzure")
                    dbinit.Text = "$('#databaseOptionBlank').click();$('#databaseOptionBlank').change();";
                //toggleVisible(DatabaseConnectionString, ManualConnectionString);

            // Make sure ASP.Net displays the password text
            DatabasePassword.Attributes["value"] = DatabasePassword.Text;
        }

        /// <summary>
        /// Shows the installation/upgrade panel.
        /// </summary>
        protected void saveDBConfig(object sender, EventArgs e)
        {
            Helper.setProgress(5, "Saving database connection...", "");

            try
            {
                DbConnectionStringBuilder connectionStringBuilder = CreateConnectionString();
                GlobalSettings.DbDSN = connectionStringBuilder.ConnectionString;
            }
            catch (Exception ex)
            {
                Exception error = new Exception("Could not save the web.config file. Please modify the connection string manually.", ex);
                Helper.setProgress(-1, "Could not save the web.config file. Please modify the connection string manually.", error.InnerException.Message);
            }

            settings.Visible = false;
            installing.Visible = true;
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
            else if (!IsEmbeddedDatabase && !DatabaseType.SelectedValue.Contains("SqlAzure"))//If database is not embedded or of type Sql Azure
            {
                connectionStringBuilder["server"] = DatabaseServer.Text;
                connectionStringBuilder["database"] = DatabaseName.Text;
                connectionStringBuilder["user id"] = DatabaseUsername.Text;
                connectionStringBuilder["password"] = DatabasePassword.Text;
            }
            else if (!IsEmbeddedDatabase && DatabaseType.SelectedValue.Contains("SqlAzure"))//If database is not embedded and of type Sql Azure
            {
                connectionStringBuilder.ConnectionString =
                    string.Format("Server=tcp:{0}.database.windows.net;Database={1};User ID={2}@{0};Password={3}",
                                  DatabaseServer.Text, DatabaseName.Text, DatabaseUsername.Text, DatabasePassword.Text);
            }
            else if (Request["database"] == "embedded")
            {
                connectionStringBuilder.ConnectionString = @"datalayer=SQLCE4Umbraco.SqlCEHelper,SQLCE4Umbraco;data source=|DataDirectory|\Umbraco.sdf";
            }

            if (!String.IsNullOrEmpty(Request["database"]) && !String.IsNullOrEmpty(DatabaseType.SelectedValue) 
                && !DatabaseType.SelectedValue.Contains("SqlServer")
                && !DatabaseType.SelectedValue.Contains("SqlAzure")
                && Request["database"] != "advanced")
            {
                connectionStringBuilder["datalayer"] = DatabaseType.SelectedValue;
            }

            //if (!String.IsNullOrEmpty(DatabaseType.SelectedValue) && !DatabaseType.SelectedValue.Contains("SqlServer") && !DatabaseType.SelectedValue.Contains("Custom"))
            //    connectionStringBuilder["datalayer"] = DatabaseType.SelectedValue;

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
            toggleVisible(DatabaseServerItem, !ManualConnectionString && !IsEmbeddedDatabase);
            toggleVisible(DatabaseUsernameItem, !ManualConnectionString && !IsEmbeddedDatabase);
            toggleVisible(DatabasePasswordItem, !ManualConnectionString && !IsEmbeddedDatabase);
            toggleVisible(DatabaseNameItem, !ManualConnectionString && !IsEmbeddedDatabase);
            
            //toggleVisible(DatabaseConnectionString, ManualConnectionString);
        }

        private void toggleVisible(HtmlGenericControl div, bool visible)
        {
            if (!visible)
                div.Attributes["style"] = "display: none;";
            else
                div.Attributes["style"] = "display: block;";
        }

        protected void gotoSettings(object sender, EventArgs e)
        {
            settings.Visible = true;
            installing.Visible = false;

            ShowDatabaseSettings();

            jsVars.Text = "showDatabaseSettings();";
        }
        
        protected void gotoNextStep(object sender, EventArgs e)
        {
            Helper.RedirectToNextStep(this.Page);
        }
    }
}