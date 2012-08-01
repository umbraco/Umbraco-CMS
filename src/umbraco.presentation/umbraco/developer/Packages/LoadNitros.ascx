<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoadNitros.ascx.cs" Inherits="umbraco.presentation.developer.packages.LoadNitros" %>

<asp:Panel id="loadNitros" runat="server">

<div id="list1a">
<span id="editorCategories">
<a class="accordianOpener">
Editors picks
<small>Recommended by the umbraco core team</small>
</a>
<div style="display: block;" class="accordianContainer">
<asp:PlaceHolder ID="ph_recommendedHolder" runat="server" />
</div>
</span>

<span id="generatedCategories">
<asp:Repeater ID="rep_nitros" runat="server" OnItemDataBound="onCategoryDataBound">
<ItemTemplate>
<a class="accordianOpener generated">
<asp:Literal ID="lit_name" runat="server" />
<small><asp:Literal ID="lit_desc" runat="server"/></small>
</a>
<div class="accordianContainer generated">
  <asp:PlaceHolder ID="ph_nitroHolder" runat="server" />
</div>
</ItemTemplate>
</asp:Repeater>
</span>
</div>

<asp:Button runat="server" CssClass="loadNitrosButton" id="bt_install" OnClick="installNitros" OnClientClick="InstallPackages(this,'loadingBar'); return true;" Text="Install selected modules" />
</asp:Panel>