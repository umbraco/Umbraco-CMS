<%@ Control Language="c#" AutoEventWireup="True" CodeBehind="database.ascx.cs" Inherits="umbraco.presentation.install.steps.detect"
    TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>


<asp:Placeholder ID="settings" runat="server" Visible="true">
        <!-- database box -->
        <div class="tab main-tabinfo">
            <div class="container">
                <h1>
                    Database configuration</h1>
                <p>
                    To complete this step, you must know some information regarding your database server
                    (”connection string”). Please contact your ISP if necessary. If you’re installingon
                    a local machine or server you might need information from your system administrator.</p>
                <p>
                    <strong>If you do not have any database available,</strong> you can choose the embedded
                    database which does not require any information from you ISP or system administrators.</p>
            </div>
            <!-- database -->
            <div class="database-hold">
                <form action="#">
                <fieldset>
                    <div class="step">
                        <div class="container">
                            <p>
                                <strong>1. Do you have a blank database already installed?</strong></p>
                            <ul class="mini-tabset">
                                <li class="btn-yes"><a href="#database-step1" class="database-tab"><span>yes</span></a></li>
                                <li class="btn-no"><a href="#database-step2" class="database-tab"><span>no</span></a></li>
                            </ul>
                        </div>
                    </div>

                    <!-- step1 -->
                    <div id="database-step1">
                        <div class="step">
                            <div class="container">
                                <p>
                                    <strong>2. Now choose your database type below.</strong></p>
                                <div class="row select">
                                        <asp:DropDownList runat="server" ID="DatabaseType" CssClass="sel">
                                            <asp:ListItem Value="" Text="Please choose" Selected="True" />
                                            <asp:ListItem Value="SqlServer" Text="Microsoft SQL Server" />
                                            <asp:ListItem Value="MySql" Text="MySQL" />
                                            <asp:ListItem Value="Custom" Text="Custom connection" />
                                            <asp:ListItem Value="SQLCE4Umbraco.SqlCEHelper,SQLCE4Umbraco" Text="SQL CE 4 (beta)" />
                                        </asp:DropDownList>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- step1-2 -->
                    <div id="database-step1-2">
                        <div class="step">
                            <div class="container">
                                <p>
                                    <strong>3. INSTRUCTIONS:</strong> Please fill out the below fields to connect to
                                    your database.</p>
                                <div class="instruction-hold">
                                    <div class="row embedded" runat="server" id="embeddedFilesMissing" style="display: none;">
                                        <p>
                                        SQL CE 4 requires that you manually add the SQL CE 4 runtime to your Umbraco installation.<br />
                                        <a href="http://our.umbraco.org/wiki/install-and-setup/using-sql-ce-4-with-juno" target="_blank">More instructions</a>.
                                        <br />
                                         You can either follow the instructions above to add SQL CE 4 or choose another database.
                                        </p>
                                    </div>
                                    
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
                                    
                                    <div class="row custom" runat="server" id="DatabaseConnectionString">
                                        <asp:Label runat="server" AssociatedControlID="ConnectionString" ID="ConnectionStringLabel">Connection string:</asp:Label>
                                        <span>
                                            <asp:TextBox runat="server" CssClass="text" ID="ConnectionString" /></span>
                                        <p>
                                            <small></small>Example: <tt>datalayer=MySQL;server=192.168.2.8;user id=user;password=***;database=umbraco</tt></p>
                                    </div>
                                </div>
                            </div>
                            <!-- btn box -->
                            <footer class="btn-box">
	                        <div class="t">&nbsp;</div>
                                <asp:LinkButton runat="server" class="single-tab submit btn-install" onclick="DatabaseConnectButton_Click"><span>install</span</asp:LinkButton>
							</footer>
                        </div>
                    </div>


                    <!-- step2 -->
                    <div class="step" id="database-step2">
                        <div class="container">
                            <p>
                                <strong>2. Nodio dignissimos ducimus qui blanditiis praesentium.</strong><br />
                                Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque
                                laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi
                                architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas
                                sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione
                                voluptatem sequi nesciunt:
                            </p>
                            <span class="btn-link"><a href="#">www.link.com</a></span>
                        </div>
                    </div>
                </fieldset>
                </form>
            </div>
        </div>

        <script type="text/javascript">
            var hasEmbeddedDlls = <%= HasEmbeddedDatabaseFiles.ToString().ToLower() %>;
            var currentVersion = '<%=umbraco.GlobalSettings.CurrentVersion%>';
            var configured = <%= IsConfigured.ToString().ToLower() %>;

            <asp:literal runat="server" id="jsVars" />
        </script>

    </asp:Placeholder>


    <asp:Panel ID="identify" runat="server" Visible="false">
        <h1>
            <asp:Literal ID="dbEmpty" runat="server" Visible="false">Database connection succeeded</asp:Literal>
        </h1>


        <asp:placeholder ID="installed" runat="server" Visible="False">
            <h1>Database installed</h1>
            <p>Your current database is up-to-date!. Click <strong>Continue</strong> to continue the configuration wizard</p>
            <p>
                No action was needed, as your database was fully up-to-date.
            </p>

            <footer class="btn-box">
	        <div class="t">&nbsp;</div>
                <asp:LinkButton class="btn-step btn btn-continue" runat="server" OnClick="gotoNextStep"><span>Continue</span></asp:LinkButton>
	        </footer>

        </asp:placeholder>
        
        <asp:PlaceHolder ID="other" runat="server" Visible="False">
                <p>
                    Press the <strong>install</strong> button to upgrade your database to Umbraco
                    <%=umbraco.GlobalSettings.CurrentVersion%></p>
                <p>
                    Don't worry - no content will be deleted and everything will continue working afterwards!
                </p>
                
                       <footer class="btn-box">
	        <div class="t">&nbsp;</div>
               <asp:LinkButton runat="server" class="single-tab submit btn-install" OnClick="upgrade_Click">><span>Install</span</asp:LinkButton>
	        </footer>     
        </asp:PlaceHolder>


        <asp:PlaceHolder ID="none" runat="server" Visible="False">
          <p>
                    Press the <strong>install</strong> button to install the Umbraco
                    <%=umbraco.GlobalSettings.CurrentVersion%>
                    database</p>

                    <footer class="btn-box">
	        <div class="t">&nbsp;</div>
               <asp:LinkButton runat="server" class="single-tab submit btn-install"  OnClick="install_Click"><span>Install</span</asp:LinkButton>
	        </footer>
         </asp:PlaceHolder>


        <asp:PlaceHolder ID="error" runat="server" Visible="False">
        <p>Database not found! Please check that the information in the "connection string" of the “web.config” file is correct.</p>
        <p>To proceed, please edit the "web.config" file (using Visual Studio or your favourite text editor), scroll to the bottom, add the connection string for your database in the key named "umbracoDbDSN" and save the file. </p>
        <p>
        Click the <strong>retry</strong> button when 
        done.<br /><a href="http://umbraco.org/redir/installWebConfig" target="_blank">
			        More information on editing web.config here.</a></p>
        
            <!-- btn box -->
            <footer class="btn-box">
	        <div class="t">&nbsp;</div>
               <asp:LinkButton runat="server" class="single-tab submit btn-retry" ><span>retry</span</asp:LinkButton>
	        </footer>
        </asp:PlaceHolder>
        

    
    </asp:Panel>

    <asp:Panel ID="confirms" runat="server" Visible="False">
        <asp:PlaceHolder ID="installConfirm" runat="server" Visible="False">
            
            <h1>Database installed</h1>

            <div class="success">
                <p>
                    Umbraco
                    <%=umbraco.GlobalSettings.CurrentVersion%>
                    has now been copied to your database. Press <b>Continue</b> to proceed.</p>
            </div>

        </asp:PlaceHolder>

        <asp:PlaceHolder ID="upgradeConfirm" runat="server" Visible="False">
            <h1>Database upgraded</h2>
            <div class="success">
                <p>
                    Your database has been upgraded to the final version
                    <%=umbraco.GlobalSettings.CurrentVersion%>.<br />
                    Press <b>Continue</b> to proceed.
                </p>
            </div>
        </asp:PlaceHolder>

         <!-- btn box -->
         <footer class="btn-box">
	     <div class="t">&nbsp;</div>
            <asp:LinkButton class="btn-step btn btn-continue" runat="server" OnClick="gotoNextStep"><span>Continue</span></asp:LinkButton>
	     </footer>
    </asp:Panel>



