using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.presentation.install.utills;
using umbraco.providers;

namespace umbraco.presentation.install
{

    /// <summary>
    ///	Database detection step in the installer wizard.
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in the future. The UserControl Umbraco.Web.UI.Install.Steps.Database has superceded this.")]
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
                if (
                    ConfigurationManager.ConnectionStrings[
                        Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName] == null
                    ||
                    string.IsNullOrEmpty(
                        ConfigurationManager.ConnectionStrings[
                            Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName].ConnectionString))
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
            DbConnectionStringBuilder connectionStringBuilder = new DbConnectionStringBuilder();

            var databaseSettings = ConfigurationManager.ConnectionStrings[Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName];
            if (databaseSettings != null)
            {
                var dataHelper = DataLayerHelper.CreateSqlHelper(databaseSettings.ConnectionString, false);
                connectionStringBuilder.ConnectionString = dataHelper.ConnectionString;

                // Prepare data layer type
                string datalayerType = GetConnectionStringValue(connectionStringBuilder, "datalayer");
                if (datalayerType.Length > 0)
                {
                    foreach (ListItem item in DatabaseType.Items)
                        if (item.Value != String.Empty && datalayerType.Contains(item.Value))
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
                if (string.IsNullOrEmpty(ConnectionString.Text) == false)
                {
                    ApplicationContext.Current.DatabaseContext.ConfigureDatabaseConnection(ConnectionString.Text);
                }
                else if (IsEmbeddedDatabase)
                {
                    ApplicationContext.Current.DatabaseContext.ConfigureEmbeddedDatabaseConnection();
                }
                else
                {
                    ApplicationContext.Current.DatabaseContext.ConfigureDatabaseConnection(DatabaseServer.Text, DatabaseName.Text,
                                                                        DatabaseUsername.Text, DatabasePassword.Text,
                                                                        DatabaseType.SelectedValue);
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

        /// <summary>
        /// settings control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder settings;

        /// <summary>
        /// DatabaseType control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DropDownList DatabaseType;

        /// <summary>
        /// ph_dbError control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder ph_dbError;

        /// <summary>
        /// lt_dbError control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal lt_dbError;

        /// <summary>
        /// DatabaseServerItem control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl DatabaseServerItem;

        /// <summary>
        /// DatabaseServerLabel control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Label DatabaseServerLabel;

        /// <summary>
        /// DatabaseServer control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox DatabaseServer;

        /// <summary>
        /// DatabaseNameItem control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl DatabaseNameItem;

        /// <summary>
        /// DatabaseNameLabel control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Label DatabaseNameLabel;

        /// <summary>
        /// DatabaseName control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox DatabaseName;

        /// <summary>
        /// DatabaseUsernameItem control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl DatabaseUsernameItem;

        /// <summary>
        /// DatabaseUsernameLabel control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Label DatabaseUsernameLabel;

        /// <summary>
        /// DatabaseUsername control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox DatabaseUsername;

        /// <summary>
        /// DatabasePasswordItem control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl DatabasePasswordItem;

        /// <summary>
        /// DatabasePasswordLabel control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Label DatabasePasswordLabel;

        /// <summary>
        /// DatabasePassword control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox DatabasePassword;

        /// <summary>
        /// embeddedFilesMissing control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl embeddedFilesMissing;

        /// <summary>
        /// DatabaseConnectionString control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl DatabaseConnectionString;

        /// <summary>
        /// ConnectionStringLabel control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Label ConnectionStringLabel;

        /// <summary>
        /// ConnectionString control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox ConnectionString;

        /// <summary>
        /// jsVars control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal jsVars;

        /// <summary>
        /// dbinit control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal dbinit;

        /// <summary>
        /// installing control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder installing;

        /// <summary>
        /// installProgress control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder installProgress;

        /// <summary>
        /// upgradeProgress control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder upgradeProgress;
    }

    /// <summary>
    ///		Summary description for defaultUser.
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in the future. The UserControl that supercedes this is Umbraco.Web.UI.Install.Steps.DefaultUser")]
    public partial class defaultUser : System.Web.UI.UserControl
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {

        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        ///		Required method for Designer support - do not modify
        ///		the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion

        protected void changePassword_Click(object sender, System.EventArgs e)
        {
            Page.Validate();

            if (Page.IsValid)
            {
                User u = User.GetUser(0);
                MembershipUser user = Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].GetUser(0, true);
                user.ChangePassword(u.GetPassword(), tb_password.Text.Trim());

                // Is it using the default membership provider
                if (Membership.Providers[UmbracoSettings.DefaultBackofficeProvider] is UsersMembershipProvider)
                {
                    // Save user in membership provider
                    UsersMembershipUser umbracoUser = user as UsersMembershipUser;
                    umbracoUser.FullName = tb_name.Text.Trim();
                    Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].UpdateUser(umbracoUser);

                    // Save user details
                    u.Email = tb_email.Text.Trim();
                }
                else
                {
                    u.Name = tb_name.Text.Trim();
                    if (!(Membership.Providers[UmbracoSettings.DefaultBackofficeProvider] is ActiveDirectoryMembershipProvider)) Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].UpdateUser(user);
                }

                // we need to update the login name here as it's set to the old name when saving the user via the membership provider!
                u.LoginName = tb_login.Text;

                u.Save();

                if (cb_newsletter.Checked)
                {
                    try
                    {
                        System.Net.WebClient client = new System.Net.WebClient();
                        NameValueCollection values = new NameValueCollection();
                        values.Add("name", tb_name.Text);
                        values.Add("email", tb_email.Text);

                        client.UploadValues("http://umbraco.org/base/Ecom/SubmitEmail/installer.aspx", values);

                    }
                    catch { /* fail in silence */ }
                }


                if (String.IsNullOrWhiteSpace(GlobalSettings.ConfigurationStatus))
                    BasePages.UmbracoEnsuredPage.doLogin(u);

                Helper.RedirectToNextStep(this.Page);
            }
        }

        private void SubscribeToNewsLetter(string name, string email)
        {
            try
            {
                System.Net.WebClient client = new System.Net.WebClient();
                NameValueCollection values = new NameValueCollection();
                values.Add("name", name);
                values.Add("email", email);

                client.UploadValues("http://umbraco.org/base/Ecom/SubmitEmail/installer.aspx", values);

            }
            catch { /* fail in silence */ }
        }

        /// <summary>
        /// identify control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder identify;

        /// <summary>
        /// tb_name control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox tb_name;

        /// <summary>
        /// tb_email control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox tb_email;

        /// <summary>
        /// tb_login control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox tb_login;

        /// <summary>
        /// tb_password control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox tb_password;

        /// <summary>
        /// tb_password_confirm control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox tb_password_confirm;

        /// <summary>
        /// cb_newsletter control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CheckBox cb_newsletter;
    }

    [Obsolete("This class is no longer used and will be removed from the codebase in the future. The UserControl that supercedes this is Umbraco.Web.UI.Install.Steps.License")]
    public partial class license : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void gotoNextStep(object sender, EventArgs e)
        {
            Helper.RedirectToNextStep(this.Page);
        }

        /// <summary>
        /// btnNext control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.LinkButton btnNext;
    }

    [Obsolete("This class is no longer used and will be removed from the codebase in the future. The UserControl that supercedes this is Umbraco.Web.UI.Install.Steps.Renaming")]
    public partial class renaming : System.Web.UI.UserControl
    {
        private string _oldAccessFilePath = IOHelper.MapPath(SystemDirectories.Data + "/access.xml");
        private string _newAccessFilePath = IOHelper.MapPath(SystemDirectories.Data + "/access.config");
        private bool _changesNeeded = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            // check xslt extensions
            identifyResult.Text += checkExtensionPaths("xsltExtensions.config", "XSLT Extension");

            // check rest extensions
            identifyResult.Text += checkExtensionPaths("restExtensions.config", "REST Extension");

            // check access.xml file
            identifyResult.Text += checkAccessFile();

            if (_changesNeeded)
            {
                changesNeeded.Visible = true;
            }
            else
            {
                noChangedNeeded.Visible = true;
                changesNeeded.Visible = false;
            }
        }

        private string checkAccessFile()
        {
            if (!newAccessFileExist() && oldAccessFileExist())
            {
                _changesNeeded = true;
                return "<li>Access.xml found. Needs to be renamed to access.config</li>";
            }
            else
            {
                return "<li>Public Access file is all good. No changes needed</li>";
            }
        }

        private bool oldAccessFileExist()
        {
            return File.Exists(_oldAccessFilePath);
        }

        private bool newAccessFileExist()
        {
            return File.Exists(_newAccessFilePath);
        }

        private string checkExtensionPaths(string filename, string extensionName)
        {
            string tempResult = "";
            foreach (XmlNode ext in GetExtensions(filename, "ext"))
            {
                if (ext.Attributes.GetNamedItem("assembly") != null &&
                    ext.Attributes.GetNamedItem("assembly").Value.StartsWith("/bin/"))
                {
                    tempResult += String.Format("<li>{0} with Alias '{1}' has assembly reference that contains /bin/. That part needs to be removed</li>",
                        extensionName,
                        ext.Attributes.GetNamedItem("alias").Value);
                }
            }

            if (String.IsNullOrEmpty(tempResult))
            {
                tempResult = String.Format("<li>{0}s are all good. No changes needed</li>", extensionName);
            }
            else
            {
                _changesNeeded = true;
            }

            return tempResult;
        }

        private void updateExtensionPaths(string filename)
        {
            filename = IOHelper.MapPath(SystemDirectories.Config + "/" + filename);
            XmlDocument xsltExt = new XmlDocument();
            xsltExt.Load(filename);

            foreach (XmlNode ext in xsltExt.SelectNodes("//ext"))
            {
                if (ext.Attributes.GetNamedItem("assembly") != null &&
                    ext.Attributes.GetNamedItem("assembly").Value.StartsWith("/bin/"))
                {
                    ext.Attributes.GetNamedItem("assembly").Value =
                        ext.Attributes.GetNamedItem("assembly").Value.Substring(5);
                }
            }

            xsltExt.Save(filename);

        }


        protected void updateChanges_Click(object sender, EventArgs e)
        {
            bool succes = true;
            string progressText = "";

            // rename access file
            if (oldAccessFileExist())
            {
                try
                {
                    File.Move(_oldAccessFilePath, IOHelper.MapPath(SystemFiles.AccessXml));
                    progressText += String.Format("<li>Public Access file renamed</li>");
                }
                catch (Exception ee)
                {
                    progressText += String.Format("<li>Error renaming access file: {0}</li>", ee.ToString());
                    succes = false;
                }
            }

            // update rest exts
            try
            {
                updateExtensionPaths("restExtensions.config");
                progressText += "<li>restExtensions.config ensured.</li>";
            }
            catch (Exception ee)
            {
                progressText += String.Format("<li>Error updating restExtensions.config: {0}</li>", ee.ToString());
                succes = false;
            }

            // update xslt exts
            try
            {
                updateExtensionPaths("xsltExtensions.config");
                progressText += "<li>xsltExtensions.config ensured.</li>";
            }
            catch (Exception ee)
            {
                progressText += String.Format("<li>Error updating xsltExtensions.config: {0}</li>", ee.ToString());
                succes = false;
            }

            string resultClass = succes ? "success" : "error";
            resultText.Text = String.Format("<div class=\"{0}\"><p>{1}</p></div>",
                resultClass,
                progressText);
            result.Visible = true;
            init.Visible = false;
        }

        private XmlNodeList GetExtensions(string filename, string elementName)
        {

            // Load the XSLT extensions configuration
            XmlDocument xsltExt = new XmlDocument();
            xsltExt.Load(IOHelper.MapPath(SystemDirectories.Config + "/" + filename));

            return xsltExt.SelectNodes("//" + elementName);
        }

        /// <summary>
        /// init control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel init;

        /// <summary>
        /// noChangedNeeded control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel noChangedNeeded;

        /// <summary>
        /// changesNeeded control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel changesNeeded;

        /// <summary>
        /// identifyResult control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal identifyResult;

        /// <summary>
        /// updateChanges control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button updateChanges;

        /// <summary>
        /// result control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel result;

        /// <summary>
        /// resultText control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal resultText;
    }

    [Obsolete("This class is no longer used and will be removed from the codebase in the future. The UserControl that supercedes this is Umbraco.Web.UI.Install.Steps.StarterKits")]
    public partial class skinning : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!cms.businesslogic.skinning.Skinning.IsStarterKitInstalled())
                showStarterKits();
            else
                showStarterKitDesigns((Guid)cms.businesslogic.skinning.Skinning.StarterKitGuid());
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);



        }
        private void showStarterKits()
        {
            ph_starterKits.Controls.Add(new UserControl().LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKits.ascx"));

            pl_starterKit.Visible = true;
            pl_starterKitDesign.Visible = false;


        }

        public void showStarterKitDesigns(Guid starterKitGuid)
        {
            steps.Skinning.loadStarterKitDesigns ctrl = (steps.Skinning.loadStarterKitDesigns)new UserControl().LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKitDesigns.ascx");
            ctrl.ID = "StarterKitDesigns";

            ctrl.StarterKitGuid = starterKitGuid;
            ph_starterKitDesigns.Controls.Add(ctrl);

            pl_starterKit.Visible = false;
            pl_starterKitDesign.Visible = true;
        }

        public void showCustomizeSkin()
        {
            //Response.Redirect(GlobalSettings.Path + "/canvas.aspx?redir=" + this.ResolveUrl("~/") + "&umbSkinning=true&umbSkinningConfigurator=true");

            _default p = (_default)this.Page;
            p.GotoNextStep(helper.Request("installStep"));
        }

        protected void gotoNextStep(object sender, EventArgs e)
        {
            Helper.RedirectToNextStep(this.Page);
        }

        /// <summary>
        /// udp control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected UpdatePanel udp;

        /// <summary>
        /// pl_starterKit control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder pl_starterKit;

        /// <summary>
        /// ph_starterKits control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder ph_starterKits;

        /// <summary>
        /// pl_starterKitDesign control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder pl_starterKitDesign;

        /// <summary>
        /// ph_starterKitDesigns control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder ph_starterKitDesigns;
    }

    /// <summary>
    ///		Summary description for theend.
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in the future. The UserControl that supercedes this is Umbraco.Web.UI.Install.Steps.TheEnd")]
    public partial class theend : System.Web.UI.UserControl
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Update configurationStatus
            try
            {

                GlobalSettings.ConfigurationStatus = UmbracoVersion.Current.ToString(3);
                Application["umbracoNeedConfiguration"] = false;
            }
            catch (Exception)
            {
                //errorLiteral.Text = ex.ToString();
            }

            // Update ClientDependency version
            var clientDependencyConfig = new ClientDependencyConfiguration();
            var clientDependencyUpdated = clientDependencyConfig.IncreaseVersionNumber();

            if (!cms.businesslogic.skinning.Skinning.IsStarterKitInstalled())
                customizeSite.Visible = false;

        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        ///		Required method for Designer support - do not modify
        ///		the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }
        #endregion

        /// <summary>
        /// customizeSite control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl customizeSite;
    }

    /// <summary>
    ///		Summary description for validatePermissions.
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in the future. The UserControl that supercedes this is Umbraco.Web.UI.Install.Steps.ValidatePermissions")]
    public partial class validatePermissions : UserControl
    {
        private string[] permissionDirs = { SystemDirectories.Css, SystemDirectories.Config, SystemDirectories.Data, SystemDirectories.Media, SystemDirectories.Masterpages, SystemDirectories.MacroScripts, SystemDirectories.Xslt, SystemDirectories.UserControls, SystemDirectories.Preview };
        private string[] permissionFiles = { };
        private string[] packagesPermissionsDirs = { SystemDirectories.Bin, SystemDirectories.Umbraco, SystemDirectories.UserControls, SystemDirectories.Packages };

        protected void Page_Load(object sender, EventArgs e)
        {
            bool permissionsOK = true;
            bool packageOK = true;
            bool foldersOK = true;
            bool cacheOK = true;
            string valResult = "";

            // Test default dir permissions
            foreach (string dir in permissionDirs)
            {
                bool result = SaveAndDeleteFile(IOHelper.MapPath(dir + "/configWizardPermissionTest.txt"));

                if (!result)
                {
                    permissionsOK = false;
                    permSummary.Text += "<li>Directory: ./" + dir + "</li>";
                }

                // Print
                valResult += " " + dir + " : " + successOrFailure(result) + "!<br/>";
            }

            // Test default file permissions
            foreach (string file in permissionFiles)
            {
                bool result = OpenFileForWrite(IOHelper.MapPath(file));
                if (!result)
                {
                    permissionsOK = false;
                    permSummary.Text += "<li>File: " + file + "</li>";
                }

                // Print
                valResult += " " + file + " : " + successOrFailure(result) + "!<br/>";
            }
            permissionResults.Text = valResult;

            // Test package dir permissions
            string packageResult = "";
            foreach (string dir in packagesPermissionsDirs)
            {
                bool result =
                    SaveAndDeleteFile(IOHelper.MapPath(dir + "/configWizardPermissionTest.txt"));
                if (!result)
                {
                    packageOK = false;
                    permSummary.Text += "<li>Directory: " + dir + "</li>";
                }

                // Print
                packageResult += " ./" + dir + " : " + successOrFailure(result) + "!<br/>";
            }
            packageResults.Text = packageResult;

            // Test umbraco.xml file
            try
            {
                content.Instance.PersistXmlToFile();
                xmlResult.Text = "Success!";
            }
            catch (Exception ee)
            {
                cacheOK = false;
                xmlResult.Text = "Failed!";
                string tempFile = SystemFiles.ContentCacheXml;

                if (tempFile.Substring(0, 1) == "/")
                    tempFile = tempFile.Substring(1, tempFile.Length - 1);

                permSummary.Text += string.Format("<li>File ./{0}<br/><strong>Error message: </strong>{1}</li>", tempFile, ee);
            }

            // Test creation of folders
            try
            {
                string tempDir = IOHelper.MapPath(SystemDirectories.Media + "/testCreatedByConfigWizard");
                Directory.CreateDirectory(tempDir);
                Directory.Delete(tempDir);
                foldersResult.Text = "Success!";
            }
            catch
            {
                foldersOK = false;
                foldersResult.Text = "Failure!";
            }

            // update config files
            if (permissionsOK)
            {
                foreach (
                    FileInfo configFile in new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Config)).GetFiles("*.xml"))
                {
                    try
                    {
                        if (File.Exists(configFile.FullName.Replace(".xml", ".config")))
                            File.Delete(configFile.FullName.Replace(".xml", ".config"));

                        configFile.MoveTo(configFile.FullName.Replace(".xml", ".config"));
                    }
                    catch { }

                }
            }

            // Generate summary
            howtoResolve.Visible = true;
            if (permissionsOK && cacheOK && packageOK && foldersOK)
            {
                perfect.Visible = true;
                howtoResolve.Visible = false;
            }
            else if (permissionsOK && cacheOK && foldersOK)
                noPackages.Visible = true;
            else if (permissionsOK && cacheOK)
            {
                folderWoes.Visible = true;
                grant.Visible = false;
                noFolders.Visible = true;
            }
            else
            {
                error.Visible = true;
                if (!foldersOK)
                    folderWoes.Visible = true;
            }
        }

        private string successOrFailure(bool result)
        {
            if (result)
                return "Success";
            else
                return "Failure";
        }

        private bool SaveAndDeleteFile(string file)
        {
            try
            {
                //first check if the directory of the file exists, and if not try to create that first.
                FileInfo fi = new FileInfo(file);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }

                File.WriteAllText(file,
                                  "This file has been created by the umbraco configuration wizard. It is safe to delete it!");
                File.Delete(file);
                return true;
            }
            catch
            {
                return false;
            }

        }

        private bool OpenFileForWrite(string file)
        {
            try
            {
                File.AppendText(file).Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        protected void gotoNextStep(object sender, EventArgs e)
        {
            Helper.RedirectToNextStep(this.Page);
        }

        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        ///		Required method for Designer support - do not modify
        ///		the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        #endregion

        /// <summary>
        /// perfect control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal perfect;

        /// <summary>
        /// noPackages control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal noPackages;

        /// <summary>
        /// noFolders control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal noFolders;

        /// <summary>
        /// error control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal error;

        /// <summary>
        /// howtoResolve control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel howtoResolve;

        /// <summary>
        /// grant control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel grant;

        /// <summary>
        /// permSummary control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal permSummary;

        /// <summary>
        /// folderWoes control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel folderWoes;

        /// <summary>
        /// permissionResults control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal permissionResults;

        /// <summary>
        /// packageResults control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal packageResults;

        /// <summary>
        /// xmlResult control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal xmlResult;

        /// <summary>
        /// foldersResult control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal foldersResult;

        /// <summary>
        /// btnNext control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.LinkButton btnNext;
    }

    /// <summary>
    ///		Summary description for welcome.
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in the future. The UserControl that supercedes this is Umbraco.Web.UI.Install.Steps.Welcome")]
    public partial class welcome : System.Web.UI.UserControl
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            var result = ApplicationContext.Current.DatabaseContext.ValidateDatabaseSchema();
            var determinedVersion = result.DetermineInstalledVersion();

            // Display the Umbraco upgrade message if Umbraco is already installed
            if (String.IsNullOrWhiteSpace(GlobalSettings.ConfigurationStatus) == false || determinedVersion.Equals(new Version(0, 0, 0)) == false)
            {
                ph_install.Visible = false;
                ph_upgrade.Visible = true;
            }

            // Check for config!
            if (GlobalSettings.Configured)
            {
                Application.Lock();
                Application["umbracoNeedConfiguration"] = null;
                Application.UnLock();
                Response.Redirect(Request.QueryString["url"] ?? "/", true);
            }
        }

        protected void gotoNextStep(object sender, EventArgs e)
        {
            Helper.RedirectToNextStep(this.Page);
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        ///		Required method for Designer support - do not modify
        ///		the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }
        #endregion

        /// <summary>
        /// ph_install control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder ph_install;

        /// <summary>
        /// ph_upgrade control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder ph_upgrade;

        /// <summary>
        /// btnNext control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.LinkButton btnNext;
    }


}
