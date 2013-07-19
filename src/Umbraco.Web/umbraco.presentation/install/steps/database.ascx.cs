using System;
using System.Configuration;
using System.Data.Common;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Umbraco.Core;
using Umbraco.Core.Logging;
using System.IO;
using Umbraco.Core.Persistence;
using umbraco.DataLayer;
using umbraco.IO;

namespace umbraco.presentation.install.steps
{
    /// <summary>
    ///	Database detection step in the installer wizard.
    /// </summary>
    public partial class detect : System.Web.UI.UserControl
    {
        /// <summary>
        /// Returns whether the selected database is an embedded database.
        /// </summary>
        protected bool IsEmbeddedDatabase
        {
            get
            {
                var databaseSettings = ConfigurationManager.ConnectionStrings[Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName];
                var configuredDatabaseIsEmbedded = databaseSettings != null && databaseSettings.ProviderName.ToLower().Contains("SqlServerCe".ToLower());

                return Request["database"] == "embedded" || configuredDatabaseIsEmbedded;
            }
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
            get
            {
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
            {
                //If the connection string is already present in web.config we don't need to show the settings page and we jump to installing/upgrading.
                var databaseSettings = ConfigurationManager.ConnectionStrings[Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName];

                var dbIsSqlCe = false;
                if(databaseSettings != null && databaseSettings.ProviderName != null)
                    dbIsSqlCe = databaseSettings.ProviderName == "System.Data.SqlServerCe.4.0";

                var sqlCeDatabaseExists = false;
                if (dbIsSqlCe)
                {
                    var dataSource = databaseSettings.ConnectionString.Replace("Datasource", "Data Source");

                    if (dataSource.Contains(@"|\") == false)
                        dataSource = dataSource.Insert(dataSource.LastIndexOf('|') + 1, "\\");

                    dataSource = dataSource.Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
                    
                    var filePath = dataSource.Replace("Data Source=", string.Empty);

                    sqlCeDatabaseExists = File.Exists(filePath);
                }

                // Either the connection details are not fully specified or it's a SQL CE database that doesn't exist yet
                if (databaseSettings == null 
                    || string.IsNullOrWhiteSpace(databaseSettings.ConnectionString) || string.IsNullOrWhiteSpace(databaseSettings.ProviderName) 
                    || (dbIsSqlCe && sqlCeDatabaseExists == false))
                {
                    installProgress.Visible = true;
                    upgradeProgress.Visible = false;
                    ShowDatabaseSettings();
                }
                else
                {
                    //Since a connection string was present we verify whether this is an upgrade or an empty db
                    var result = ApplicationContext.Current.DatabaseContext.ValidateDatabaseSchema();
                    var determinedVersion = result.DetermineInstalledVersion();
                    if (determinedVersion.Equals(new Version(0, 0, 0)))
                    {
                        //Fresh install
                        installProgress.Visible = true;
                        upgradeProgress.Visible = false;
                    }
                    else
                    {
                        //Upgrade
                        installProgress.Visible = false;
                        upgradeProgress.Visible = true;
                    }

                    settings.Visible = false;
                    installing.Visible = true;
                }
            }
        }

        /// <summary>
        /// Prepares and shows the database settings panel.
        /// </summary>
        protected void ShowDatabaseSettings()
        {
            // Parse the connection string
            var connectionStringBuilder = new DbConnectionStringBuilder();

            var databaseSettings = ConfigurationManager.ConnectionStrings[Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName];
            if (databaseSettings != null && string.IsNullOrWhiteSpace(databaseSettings.ConnectionString) == false)
            {
                var dataHelper = DataLayerHelper.CreateSqlHelper(databaseSettings.ConnectionString, false);
                connectionStringBuilder.ConnectionString = dataHelper.ConnectionString;

                // Prepare data layer type
                var datalayerType = GetConnectionStringValue(connectionStringBuilder, "datalayer");
                if (datalayerType.Length > 0)
                {
                    foreach (ListItem item in DatabaseType.Items)
                        if (item.Value != string.Empty && datalayerType.Contains(item.Value))
                            DatabaseType.SelectedValue = item.Value;
                }
                else if (dataHelper.ConnectionString != "server=.\\SQLEXPRESS;database=DATABASE;user id=USER;password=PASS")
                    DatabaseType.SelectedValue = "SqlServer";
            }
            else
            {
                DatabaseType.SelectedValue = "SqlServer";
            }

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
            else if (ManualConnectionString)
                dbinit.Text = "$('#databaseOptionAdvanced').click();$('#databaseOptionAdvanced').change();";
            else if (DatabaseType.SelectedValue == "SqlServer")
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
            try
            {
                var dbContext = ApplicationContext.Current.DatabaseContext;

                if (string.IsNullOrEmpty(ConnectionString.Text) == false)
                {
                    dbContext.ConfigureDatabaseConnection(ConnectionString.Text);
                }
                else if (IsEmbeddedDatabase)
                {
                    dbContext.ConfigureEmbeddedDatabaseConnection();
                }
                else
                {
                    var server = DatabaseServer.Text;
                    var databaseName = DatabaseName.Text;
                    
                    if (DatabaseType.SelectedValue == "SqlServer" && DatabaseIntegratedSecurity.Checked == true)
                    {
                        dbContext.ConfigureIntegratedSecurityDatabaseConnection(server, databaseName);
                    }
                    else
                    {
                        dbContext.ConfigureDatabaseConnection(server, databaseName,
                            DatabaseUsername.Text, DatabasePassword.Text, DatabaseType.SelectedValue
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<detect>("Exception was thrown during the setup of the database in 'saveDBConfig'.", ex);
            }

            settings.Visible = false;
            installing.Visible = true;
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