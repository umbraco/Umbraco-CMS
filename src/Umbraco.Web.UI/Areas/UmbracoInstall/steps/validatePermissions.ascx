<%@ Control Language="c#" AutoEventWireup="True" Codebehind="ValidatePermissions.ascx.cs" Inherits="Umbraco.Web.UI.Install.Steps.ValidatePermissions" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<h1>Step 3/5: Validating File Permissions</h1>
<p>
umbraco needs write/modify access to certain directories in order to store files like pictures and PDF's. 
It also stores temporary data (aka: cache) for enhancing the performance of your website.
</p>

<asp:literal id="perfect" Runat="server" Visible="False">
<div class="success">
<p><strong>Your permission settings are perfect!</strong></p>
<p>You are ready to run umbraco and install packages!</p>
</div>
</asp:literal>

<asp:literal id="noPackages" Runat="server" Visible="False">
<div class="error">
<p><strong>Your permission settings are almost perfect!</strong></p>
<p>
You can run umbraco without problems, but you will not be able to install packages which are recommended to take full advantage of umbraco.
</p>
</div>
</asp:literal>

<asp:literal id="noFolders" Runat="server" Visible="False">
<div class="error">
<p>
<strong>Your permission settings might be an issue!</strong>
</p>
<p>You can run umbraco without problems, but you will not be able to create folders or install packages which are recommended to take full advantage of umbraco.</p>
</div>
</asp:literal>

<asp:literal id="error" Runat="server" Visible="False">
<div class="error">
<p>
<strong>Your permission settings are not ready for umbraco!</strong>
</p>
<p>
In order to run umbraco, you'll need to update your permission settings.
</p>
</div>
</asp:literal>

<asp:Panel Visible="False" Runat="server" ID="howtoResolve">
    <h2>How to Resolve</h2>

    <div id="fixPane" style="display: block;">
        <asp:Panel id="grant" Visible="True" Runat="server">
            <h2>Affected files and folders</h2>
            <p>
            You need to grant ASP.NET modify permissions to the following files/folders: 
            </p>

            <ul>
            <asp:Literal id="permSummary" Runat="server"></asp:Literal>
            </ul>
        </asp:Panel>

        <asp:Panel id="folderWoes" Visible="False" Runat="server">
            <h3>Resolving folder issue</h3>

            <p>Follow <a href="http://groups.google.com/group/microsoft.public.dotnet.framework.aspnet/browse_thread/thread/46d6582afffc96b0/9088396b3323d4ae?lnk=st">this link for more information on problems with ASP.NET and creating 
				            folders</a>.
            </p>
        </asp:Panel>


        <p><a href="#" onClick="javascript:document.getElementById('detailsPane').style.display = 'block';"><strong>View Permission Error Details:</strong></a></p>
        <div id="detailsPane" style="display: none">
	        <p style="font-size: 80%">
		        - Checking default folder permissions:<br />
		        <asp:literal id="permissionResults" Runat="server"></asp:literal><br />
		        - Checking optional folder permissions for packages:<br />
		        <asp:literal id="packageResults" Runat="server"></asp:literal><br />
		        - Cache (the umbraco.config file):
		        <asp:literal id="xmlResult" Runat="server"></asp:literal><br />
		        <br />
		        - Creating folders:
		        <asp:literal id="foldersResult" Runat="server"></asp:literal><br />
		        <br />
	        </p>
        </div>
    </div>
</asp:Panel>

<footer class="btn-box">
		<div class="t">&nbsp;</div>
        <asp:LinkButton ID="btnNext" CssClass="btn btn-accept" runat="server" OnClick="GotoNextStep"><span>Continue anyway</span></asp:LinkButton>
	</footer>