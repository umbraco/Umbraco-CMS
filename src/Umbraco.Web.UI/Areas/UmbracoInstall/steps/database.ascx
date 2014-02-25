<%@ Control Language="c#" AutoEventWireup="True" CodeBehind="database.ascx.cs" Inherits="Umbraco.Web.UI.Install.Steps.Database" %>
<%@ Import Namespace="Umbraco.Core.Configuration" %>

<asp:PlaceHolder ID="settings" runat="server" Visible="true">
    <!-- database box -->
    <div class="tab main-tabinfo">
        <div class="container">
            <h1>Database configuration</h1>
            <p>
                <strong>To complete this step you will either need a blank database or, if you do not have a blank database available, choose the SQL CE 4 embedded
				database (This is the recommended approach for first time users or if you are unsure).</strong>
            </p>
            <p>
                If you are not using the SQL CE 4 embedded database you will need the connection details for your database, such as the
				&quot;connection string&quot;. You may need to contact your system administrator or web host for this information.
            </p>
        </div>
        <!-- database -->
        <div class="database-hold">
                <fieldset>
                    <div class="step">
                        <div class="container">
                            <p>
                                <strong>1. Select which database option best fits you:</strong>
                            </p>

                            <ul>
                                <li>
                                    <input type="radio" id="databaseOptionEmbedded" name="database" value="embedded" />
                                    <label for="databaseOptionEmbedded">I want to use SQL CE 4, a free, quick-and-simple embedded database</label>

                                </li>
                                <li>
                                    <input type="radio" id="databaseOptionBlank" name="database" value="blank" />
                                    <label for="databaseOptionBlank">I already have a blank SQL Server, SQL Azure or MySQL database</label>

                                </li>
                                <li>
                                    <input type="radio" id="databaseOptionAdvanced" name="database" value="advanced" />
                                    <label for="databaseOptionAdvanced">I'm an advanced user, let me put in the connection string</label>

                                </li>
                                <li>
                                    <input type="radio" id="databaseOptionHelp" name="database" value="help" />
                                    <label for="databaseOptionHelp">I need help</label>

                                </li>
                            </ul>

                        </div>
                    </div>
                    <!-- database options -->
                    <div id="database-options">


                        <!-- blank option -->
                        <div id="database-blank" class="database-option">
                            <script type="text/javascript">
                                (function($) {
                                    $(document).ready(function() {
                                        // Make database username and password dependent on the integrated security option
                                        var databaseTypeSelect = $("#<%= DatabaseType.ClientID %>");
                                        var integratedSecurityCheckBox = $("#<%= DatabaseIntegratedSecurity.ClientID %>");
                                        var databaseUserNamePasswordInputs = $("#<%= DatabaseUsername.ClientID %>, #<%= DatabasePassword.ClientID %>");

                                        toggle();

                                        databaseTypeSelect.change(toggle);
                                        integratedSecurityCheckBox.change(toggle);

                                        function toggle() {
                                            var databaseType = databaseTypeSelect.val();
                                            
                                            if (databaseType == "SqlServer") {
                                                // Only show and enable the integrated security option when it's supported
                                                integratedSecurityCheckBox
                                                    .removeAttr("disabled")
                                                    .closest("div").show();
                                                
                                            } else {
                                                integratedSecurityCheckBox
                                                    .attr("disabled", "disabled")
                                                    .closest("div").hide();
                                            }

                                            if (integratedSecurityCheckBox.is(":checked")) {
                                                // Hide username and password when integrated security is checked
                                                databaseUserNamePasswordInputs.attr("disabled", "disabled");
                                                databaseUserNamePasswordInputs.closest("div").hide();
                                            } else {
                                                databaseUserNamePasswordInputs.removeAttr("disabled");
                                                databaseUserNamePasswordInputs.closest("div").show();
                                            }
                                        }
                                    });
                                })(jQuery);
                            </script>

                            <div class="step">
                                <div class="container">
                                    <p>
                                        <strong>2. Now choose your database type below.</strong>
                                    </p>
                                    <div class="select">
                                        <asp:DropDownList runat="server" ID="DatabaseType" CssClass="sel">
                                            <asp:ListItem Value="" Text="Please choose" Selected="True" />
                                            <asp:ListItem Value="SqlServer" Text="Microsoft SQL Server" />
                                            <asp:ListItem Value="SqlAzure" Text="SQL Azure" />
                                            <asp:ListItem Value="MySql" Text="MySQL" />

                                        </asp:DropDownList>
                                    </div>
                                </div>
                            </div>


                            <div class="step" id="database-blank-inputs">
                                <div class="container">
                                    <p class="instructionText">
                                        <strong>3. Connection details:</strong> Please fill out the connection information for your database.
                                    </p>
                                    <div class="instruction-hold">


                                        <asp:PlaceHolder ID="ph_dbError" runat="server" Visible="false">
                                            <div class="row error">
                                                <p class="text">
                                                    <strong>
                                                        <asp:Literal ID="lt_dbError" runat="server" /></strong>
                                                </p>
                                            </div>
                                            <script type="text/javascript">
                                                jQuery(document).ready(function () { showDatabaseSettings(); });
                                            </script>
                                        </asp:PlaceHolder>
                                        <div class="row sql" runat="server" id="DatabaseServerItem">
                                            <asp:Label runat="server" AssociatedControlID="DatabaseServer" ID="DatabaseServerLabel">Server:</asp:Label>
                                            <span>
                                                <asp:TextBox runat="server" CssClass="text" ID="DatabaseServer" /></span>
                                        </div>
                                        <div class="row sql" runat="server" id="DatabaseNameItem">
                                            <asp:Label runat="server" AssociatedControlID="DatabaseName" ID="DatabaseNameLabel">Database name:</asp:Label>
                                            <span>
                                                <asp:TextBox runat="server" CssClass="text" ID="DatabaseName" /></span>
                                        </div>
                                        
                                        <div class="row sql">
                                            <asp:Label runat="server" AssociatedControlID="DatabaseIntegratedSecurity">Integrated security:</asp:Label>
                                            <asp:CheckBox runat="server" ID="DatabaseIntegratedSecurity" />
                                        </div>

                                        <div class="row sql" runat="server" id="DatabaseUsernameItem">
                                            <asp:Label runat="server" AssociatedControlID="DatabaseUsername" ID="DatabaseUsernameLabel">Username:</asp:Label>
                                            <span>
                                                <asp:TextBox runat="server" CssClass="text" ID="DatabaseUsername" /></span>
                                        </div>
                                        <div class="row sql" runat="server" id="DatabasePasswordItem">
                                            <asp:Label runat="server" AssociatedControlID="DatabasePassword" ID="DatabasePasswordLabel">Password:</asp:Label>
                                            <span>
                                                <asp:TextBox runat="server" ID="DatabasePassword" CssClass="text" TextMode="Password" /></span>
                                        </div>

                                    </div>
                                </div>
                                <!-- btn box -->

                            </div>
                        </div>

                        <!-- embedded option -->
                        <div id="database-embedded" class="database-option">
                            <div class="step">
                                <div class="container">

                                    <p class="instructionText">
                                        <strong>2. Simple file-based database:</strong>
                                    </p>
                                    <div class="instruction-hold">
                                        <div class="row embeddedError" runat="server" id="embeddedFilesMissing" style="display: none;">
                                            <p>
                                                <strong>Missing files:</strong> SQL CE 4 requires that you manually add the SQL
										CE 4 runtime to your Umbraco installation.<br />
                                                You can either use the following <a href="http://our.umbraco.org/wiki/install-and-setup/using-sql-ce-4-with-umbraco-46"
                                                    target="_blank">instructions</a> on how to add SQL CE 4 or select another database type from the dropdown above.
                                            </p>
                                        </div>

                                        <div class="row embedded" style="display: none;">
                                            <p>
                                                <strong>Nothing to configure:</strong>SQL CE 4 does not require any configuration,
                                        simply click the "install" button to continue.
                                            </p>
                                        </div>

                                    </div>
                                </div>
                            </div>
                        </div>
                        <!-- advanced option -->
                        <div id="database-advanced" class="database-option">
                            <div class="step">
                                <div class="container">
                                    <p>
                                        <strong>2. Connection details:</strong> Please fill out the connection information for your database.
                                    </p>

                                    <div class="instruction-hold">

                                        <div class="row custom" runat="server" id="DatabaseConnectionString">
                                            <asp:Label runat="server" AssociatedControlID="ConnectionString" ID="ConnectionStringLabel">Connection string:</asp:Label>
                                            <span class="textarea">
                                                <asp:TextBox runat="server" TextMode="MultiLine" CssClass="text textarea" ID="ConnectionString" /></span>
                                        </div>
                                        <div class="row custom check-hold">
                                            <p>
                                                Example: <tt>datalayer=MySQL;server=192.168.2.8;user id=user;password=***;database=umbraco</tt>
                                            </p>
                                        </div>

                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- help option -->
                        <div id="database-help" class="database-option">
                            <div class="step">
                                <div class="container">
                                    <p>
                                        <strong>2. Getting a database setup for umbraco.</strong><br />
                                        For first time users, we recommend you select "quick-and-simple embedded database".
                                    This will install an easy to use database, that does
                                    not require any additional software to use.<br />
                                        Alternatively, you can install Microsoft SQL Server, which will require a bit more
                                    work to get up and running.<br />
                                        We have provided a step-by-step guide in the video instructions below.
                                    </p>
                                    <span class="btn-link"><a href="http://umbraco.org/getting-started" target="_blank">Open video instructions</a></span>
                                </div>
                            </div>
                        </div>

                        <footer class="btn-box installbtn">
                            <div class="t">&nbsp;</div>
                            <asp:LinkButton runat="server" class="single-tab submit btn-install" OnClick="SaveDbConfig"><span>install</span>    </asp:LinkButton>
                        </footer>

                    </div>



                </fieldset>
        </div>
    </div>
    <script type="text/javascript">
        var currentVersion = '<%=UmbracoVersion.Current.ToString(3)%> <%=UmbracoVersion.CurrentComment%> ';
        var configured = <%= IsConfigured.ToString().ToLower() %>;

        jQuery(document).ready(function(){
            <asp:literal runat="server" id="jsVars" />



            $("input[name='database']").change(function()
            {

                switch($(this).val())
                {
                case "blank":

                    $(".database-option").hide();
                    $("#database-blank").show();
                    $(".installbtn").show();
	                   
                    break;
                case "embedded":
                    $(".database-option").hide();
                    $("#database-embedded").show();

                    $('.embedded').show();
                    $(".installbtn").show();
                    
                    break;
                case "advanced":
                    $(".database-option").hide();
                    $("#database-advanced").show();
                    $(".installbtn").show();
                    break;
                case "help":
                    $(".database-option").hide();
                    $("#database-help").show();
                    $(".installbtn").hide();
                    break;
                }


            });

            <asp:Literal id="dbinit" runat="server"></asp:Literal>

        });
    </script>
</asp:PlaceHolder>

<asp:PlaceHolder ID="installing" runat="server" Visible="false">
    <!-- installing umbraco -->
    <div class="tab  install-tab" id="datebase-tab">
        <div class="container">
            <asp:PlaceHolder ID="installProgress" runat="server" Visible="True">
                <div class="progress-status-container">
                    <h1>Installing Umbraco</h1>
                    <p>
                        The Umbraco database is being configured. This process populates your chosen database with a blank Umbraco instance.
                    </p>
                </div>
                <div class="result-status-container" style="display: none;">
                    <h1>Database installed</h1>
                    <div class="success">
                        <p>
                            Umbraco
				            <%=UmbracoVersion.Current.ToString(3)%> <%=UmbracoVersion.CurrentComment%>
				            has now been copied to your database. Press <b>Continue</b> to proceed.
                        </p>
                    </div>
                </div>
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="upgradeProgress" runat="server" Visible="False">
                <div class="progress-status-container">
                    <h1>Upgrading Umbraco</h1>
                    <p>
                        The Umbraco database is being configured. This process upgrades your Umbraco database.
                    </p>
                </div>
                <div class="result-status-container" style="display: none;">
                    <h1>Database upgraded</h1>
                    <div class="success">
                        <p>
                            Your database has been upgraded to version: 
					            <%=UmbracoVersion.Current.ToString(3)%>.<br />
                            Press <b>Continue</b> to proceed.
                        </p>
                    </div>                    
                </div>
            </asp:PlaceHolder>
            <div class="loader">
                <div class="hold">
                    <div class="progress-bar">
                    </div>
                    <span class="progress-bar-value">0%</span>
                </div>
                <strong></strong>
            </div>
        </div>
        <!-- btn box -->
        <footer class="btn-box" style="display: none;">
            <div class="t">&nbsp;</div>
            <asp:LinkButton class="btn-step btn btn-continue" runat="server" OnClick="GotoNextStep"><span>Continue</span></asp:LinkButton>
            <asp:LinkButton class="btn-step btn btn-back" Style="display: none;" runat="server" OnClick="GotoSettings"><span>Back</span></asp:LinkButton>
        </footer>
    </div>

    <script type="text/javascript">
        jQuery(document).ready(function() {
            
            updateProgressBar("5");
            updateStatusMessage("Connecting to database..");

            var upgradeTimeout;

            function upgradeProgress(currProgress) {
                if (currProgress < 90) {
                    upgradeTimeout = setTimeout(function() {
                        currProgress++;
                        updateProgressBar(currProgress.toString());
                        upgradeProgress(currProgress);
                    }, 10000);
                }
            }

            function handleSuccess(json) {
                if (json.Success) {    
                    $(".btn-box").show();
                    $('.ui-progressbar-value').css("background-image", "url(../umbraco_client/installer/images/pbar.gif)");
                    $(".result-status-container").show();
                    $(".progress-status-container").hide();
                } 
                else {
                    $(".btn-continue").hide();
                    $(".btn-back").show();
                    $(".btn-box").show();
                }
            }

            function runUpgrade() {
                $.ajax({
                    type: 'POST',
                    contentType: 'application/json; charset=utf-8',
                    data: '{}',
                    dataType: 'json',
                    url: 'InstallerRestService.aspx/Upgrade',
                    success: function(data) {
                        clearTimeout(upgradeTimeout);
                        var json = JSON.parse(data.d);

                        updateProgressBar(json.Percentage);
                        updateStatusMessage(json.Message);
                
                        handleSuccess(json);
                    }
                });
                upgradeProgress(30);
            }

            function runInstall() {
                $.ajax({
                    type: 'POST',
                    contentType: 'application/json; charset=utf-8',
                    data: '{}',
                    dataType: 'json',
                    url: 'InstallerRestService.aspx/Install',
                    success: function(data) {
                        var json = JSON.parse(data.d);

                        updateProgressBar(json.Percentage);
                        updateStatusMessage(json.Message);
                
                        if (json.RequiresUpgrade) {
                            runUpgrade();
                        }
                        else {
                            handleSuccess(json);
                        }
                    }
                });
            }

            //kick it off
            runInstall();
        });
    </script>

</asp:PlaceHolder>