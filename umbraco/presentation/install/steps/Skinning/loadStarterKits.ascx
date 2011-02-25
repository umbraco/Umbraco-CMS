<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="loadStarterKits.ascx.cs" Inherits="umbraco.presentation.install.steps.Skinning.loadStarterKits" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>

<asp:PlaceHolder ID="pl_loadStarterKits" runat="server">
<asp:Repeater ID="rep_starterKits" runat="server">
	<HeaderTemplate>
		<nav class="zoom-list add-nav">
		<ul>
	</HeaderTemplate>
	<ItemTemplate>

    <li class="add-<%# ((Package)Container.DataItem).Text.Replace(" ","").ToLower() %>">
        <asp:LinkButton CssClass="single-tab selectStarterKit" ID="bt_selectKit" runat="server" onclick="SelectStarterKit" ToolTip="<%# ((Package)Container.DataItem).Text %>" CommandArgument="<%# ((Package)Container.DataItem).RepoGuid %>">
        <img class="zoom-img" src="<%# ((Package)Container.DataItem).Thumbnail %>" alt="<%# ((Package)Container.DataItem).Text %>" width="150" height="204"></asp:LinkButton>
	<em>&nbsp;</em>
	<!-- drop down -->
	<div class="drop-hold">
		<div class="t">&nbsp;</div>
		<div class="c">
			<div class="title">
				<span><strong><%# ((Package)Container.DataItem).Text %></strong> contains the following functionality</span>
			</div>
			<div class="hold">
				<%# ((Package)Container.DataItem).Description %>
			</div>
		</div>
		<div class="b">&nbsp;</div>
	</div>
    </li>
	</ItemTemplate>
	<FooterTemplate>
	
    <li class="add-thanks">
        <asp:LinkButton runat="server" class="single-tab declineStarterKits" ID="declineStarterKits" OnClientClick="return confirm('Are you sure you do not want to install a starter kit?');" OnClick="NextStep">
            <img class="zoom-img" src="../umbraco_client/installer/images/btn-no-thanks.png" alt="image description" width="150" height="204">
        </asp:LinkButton>

		<em>&nbsp;</em>
		<!-- drop down -->
		<div class="drop-hold">
			<div class="t">&nbsp;</div>
			<div class="c">
				<div class="title">
					<span><strong>Choose not to install a stater kit</strong></span>
				</div>
			</div>
			<div class="b">&nbsp;</div>
		</div>
	</li>

	</ul>
	</nav>    
	</FooterTemplate>
</asp:Repeater>
</asp:PlaceHolder>

<asp:PlaceHolder ID="pl_starterKitsConnectionError" runat="server" Visible="false">
	
        <div style="padding: 0 100px 13px 5px;">
            <h2>Oops...the installer can't connect to the repository</h2> 
            Starter Kits could not be fetched from the repository as there was no connection - which can occur if you are using a proxy server or firewall with certain configurations,
            or if you are not currently connected to the internet.
            <br />
            Click <strong>Continue</strong> to complete the installation then navigate to the Developer section of your Umbraco installation
            where you will find the Starter Kits listed in the Packages tree.
        </div>
    
    <!-- btn box -->
	<footer class="btn-box">
         <div class="t">&nbsp;</div>
         <asp:LinkButton ID="LinkButton1" class="btn-step btn btn-continue" runat="server" onclick="gotoLastStep"><span>Continue</span></asp:LinkButton>
    </footer>
</asp:PlaceHolder>