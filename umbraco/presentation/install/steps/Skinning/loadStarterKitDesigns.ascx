<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="loadStarterKitDesigns.ascx.cs" Inherits="umbraco.presentation.install.steps.Skinning.loadStarterKitDesigns" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>

<asp:PlaceHolder ID="pl_loadStarterKitDesigns" runat="server">
<asp:Panel id="pl_CustomizeSkin" runat="server" Visible="false">
<h3>Starter kit and skin have been installed</h3>
<p id="customizelink"><a target="_blank" href="<%= umbraco.GlobalSettings.Path %>/canvas.aspx?redir=<%= this.ResolveUrl("~/")  %>&umbSkinning=true&umbSkinningConfigurator=true" target="_blank">Browse and customize your new site</a></p>
</asp:Panel>

<div id="skinselector">
<asp:Repeater ID="rep_starterKitDesigns" runat="server">
	<HeaderTemplate>
		<!-- gallery -->
		<div class="gallery">
		<a href="#" class="btn-prev"><span>prev</span></a>
		<a href="#" class="btn-next"><span>next</span></a>

		<div class="hold">
		<div class="gal-box">

		<div class="box zoom-list2">
		<ul>
	</HeaderTemplate>

	
	<ItemTemplate>
	<li>
	<div class="image-hold">
	<img class="faik-mask" src="../umbraco_client/installer/images/bg-img.png" alt="image description" width="152" height="129">
	<img class="faik-mask-ie6" src="../umbraco_client/installer/images/bg-img-ie.png" alt="image description" width="201" height="178">

	<div class="image">
	<img class="zoom-img" src="<%# ((Skin)Container.DataItem).Thumbnail %>" alt="<%# ((Skin)Container.DataItem).Text %>" width="134" height="103">
			<div class="gal-drop">
				<a href="#lightbox" class="btn-preview" title="<%# ((Skin)Container.DataItem).Text %>"><span>Preview</span></a>
				<asp:LinkButton CssClass="single-tab btn-install-gal" ID="bt_selectKit" runat="server" onclick="SelectStarterKitDesign" CommandArgument="<%# ((Skin)Container.DataItem).RepoGuid %>" ToolTip="<%# ((Skin)Container.DataItem).Text %>"><span>Install</span></asp:LinkButton>
				<div class="gal-desc" style="display: none"><%# ((Skin)Container.DataItem).Description %></div>
				<div class="gal-owner" style="display: none">Created by: <a href="<%# ((Skin)Container.DataItem).AuthorUrl %>" target="_blank"><%# ((Skin)Container.DataItem).Author %></a></div>
			</div>
	</div>

	</div>
	</li>
	</ItemTemplate>
	<FooterTemplate>
		</ul>
		</div>
		
		<!-- paging -->
		<div class="paging">
		<div class="w1">
				<div class="w2">
				<span>Pages:</span>
				<ul class="swicher">
						<li class="active"><a href="#">1</a></li>
						<li><a href="#">2</a></li>
						<li><a href="#">3</a></li>
						<li><a href="#">4</a></li>
					</ul>
				</div>
			</div>
		</div>

		</div></div></div>
	</FooterTemplate>
</asp:Repeater>
</div>

</asp:PlaceHolder>