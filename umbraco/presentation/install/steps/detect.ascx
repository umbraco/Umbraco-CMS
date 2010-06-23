<%@ Control Language="c#" AutoEventWireup="True" Codebehind="detect.ascx.cs" Inherits="umbraco.presentation.install.steps.detect"
  TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<h1>Step 2/5: Database configuration</h1>
<asp:Panel ID="settings" runat="server" Visible="true">
    
        
        <h2>Database configuration</h2>
        <div runat="server" id="div_info" class="notice">
        <p>To complete this step, you must know some information regarding your database server ("connection string").<br />
        Please contact your ISP if necessary.
        If you're installing on a local machine or server you might need information from your system administrator.</p>
        
        <p>
        <strong>If you do not have any database available</strong>, you can choose the embedded database which does not require any information
        from your ISP or system administrators, and will install right away.
        </p>
        
        </div>
        
        <p runat="server" id="DatabaseError" class="errormessage"></p>
        
        <ol class="form">
            <li runat="server" id="DatabaseTypeItem">
                <asp:Label runat="server" AssociatedControlID="DatabaseType" ID="DatabaseTypeLabel">Type:</asp:Label>
                <asp:DropDownList runat="server" ID="DatabaseType" CssClass="textfield" 
                    AutoPostBack="True" onselectedindexchanged="DatabaseType_SelectedIndexChanged">
                    <asp:ListItem Value="SqlServer" Text="Microsoft SQL Server" Selected="True" />
                    <asp:ListItem Value="MySql" Text="MySQL" />
                    <asp:ListItem Value="" Text="Custom connection" />
                </asp:DropDownList>
            </li>
            <li runat="server" id="DatabaseServerItem">
                <asp:Label runat="server" AssociatedControlID="DatabaseServer" ID="DatabaseServerLabel">Server:</asp:Label>
                <asp:TextBox runat="server" CssClass="textfield" ID="DatabaseServer" />
            </li>
            <li runat="server" id="DatabaseNameItem">
                <asp:Label runat="server" AssociatedControlID="DatabaseName" ID="DatabaseNameLabel">Database name:</asp:Label>
                <asp:TextBox runat="server" CssClass="textfield" ID="DatabaseName" />
            </li>
            <li runat="server" id="DatabaseUsernameItem">
                <asp:Label runat="server" AssociatedControlID="DatabaseUsername" ID="DatabaseUsernameLabel">Username:</asp:Label>
                <asp:TextBox runat="server" CssClass="textfield" ID="DatabaseUsername" />
            </li>
            <li runat="server" id="DatabasePasswordItem">
                <asp:Label runat="server" AssociatedControlID="DatabasePassword" ID="DatabasePasswordLabel">Password:</asp:Label>
                <asp:TextBox runat="server" ID="DatabasePassword" CssClass="textfield"  TextMode="Password" />
            </li>
            <li runat="server" id="DatabaseConnectionString" visible="false"> 
                <asp:Label runat="server" AssociatedControlID="ConnectionString" ID="ConnectionStringLabel">Connection string:</asp:Label>
                <asp:TextBox runat="server" CssClass="textfield" ID="ConnectionString" />
                <p>Example: <tt>datalayer=MySQL;server=192.168.2.8;user id=user;password=***;database=umbraco</tt></p>
            </li>
        </ol>
        
        <p><asp:Button runat="server" ID="DatabaseConnectButton" Text="Confirm" onclick="DatabaseConnectButton_Click" OnClientClick="showProgress(this,'loadingBar'); return true;" /> </p>

</asp:Panel>
<asp:Panel ID="identify" runat="server" Visible="false">
  <h2>
  <asp:Literal ID="dbEmpty" runat="server" Visible="false">Database connection succeeded. Next step is adding umbraco tables</asp:Literal>
    <asp:PlaceHolder ID="dbUpgrade" runat="server"> <asp:Literal ID="version" runat="server"></asp:Literal></asp:PlaceHolder>
  </h2>

<asp:Literal ID="installed" runat="server" Visible="False">
  <div class="success"><p>Your current database is up-to-date!. Click <strong>next</strong> to continue the configuration wizard</p></div>
</asp:Literal>

<asp:PlaceHolder ID="other" runat="server" Visible="False">
    <div class="notice">
    <p>
    Press the <strong>upgrade</strong> button to upgrade your database to Umbraco <%=umbraco.GlobalSettings.CurrentVersion%></p>
    <p>
    Don't worry - no content will be deleted and everything will continue working afterwards!
    </p>
    </div>
</asp:PlaceHolder>

<asp:PlaceHolder ID="none" runat="server" Visible="False">
  <div class="notice">
    <p>Press the <strong>install</strong> button to install the Umbraco <%=umbraco.GlobalSettings.CurrentVersion%> database</p>
  </div>
</asp:PlaceHolder>

<asp:Literal ID="error" runat="server" Visible="False">
<div class="error">
<p>Database not found! Please check that the information in the "connection string" of the “web.config” file is correct.</p>
<p>To proceed, please edit the "web.config" file (using Visual Studio or your favourite text editor), scroll to the bottom, add the connection string for your database in the key named "umbracoDbDSN" and save the file. </p>
<p>
Click the <strong>retry</strong> button when 
done.<br /><a href="http://umbraco.org/redir/installWebConfig" target="_blank">
			More information on editing web.config here.</a></p>
</div>
</asp:Literal>
</asp:Panel>

<asp:Button ID="upgrade" Text="Upgrade" Visible="False" runat="server" CssClass="button" OnClientClick="showProgress(this,'loadingBar'); return true;"  OnClick="upgrade_Click"></asp:Button>
<asp:Button ID="install" Text="Install" Visible="False" runat="server" CssClass="button"  OnClientClick="showProgress(this,'loadingBar'); return true;" OnClick="install_Click"></asp:Button>
<asp:Button ID="retry" Text="Retry" Visible="False" runat="server" OnClientClick="showProgress(this,'loadingBar'); return true;" CssClass="button" />


<asp:Panel ID="confirms" runat="server" Visible="False">

<asp:PlaceHolder ID="installConfirm" runat="server" Visible="False">
<div class="success">
<p>Umbraco <%=umbraco.GlobalSettings.CurrentVersion%> has now been copied to your database. Press <b>Next</b> to proceed.</p></div>
</asp:PlaceHolder>

<asp:PlaceHolder ID="upgradeConfirm" runat="server" Visible="False">
<div class="success">
<p>Your database has been upgraded to the final version <%=umbraco.GlobalSettings.CurrentVersion%>.<br />Press <b>Next</b> to 
proceed. 
</p>
</div>
</asp:PlaceHolder>

</asp:Panel>
