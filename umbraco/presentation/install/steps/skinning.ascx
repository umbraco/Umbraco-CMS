<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="skinning.ascx.cs" Inherits="umbraco.presentation.install.steps.skinning" %>

<!-- Choose starter kit -->

<asp:Panel ID="pl_starterKit" Runat="server" Visible="True">


<div id="starterKits">
<asp:PlaceHolder ID="ph_starterKits" runat="server" />
</div>

</asp:Panel>

<!-- Choose starter kit design -->

<asp:Panel ID="pl_starterKitDesign" Runat="server" Visible="True">


<div id="starterKitDesigns">
<asp:PlaceHolder ID="ph_starterKitDesigns" runat="server" />
</div>

</asp:Panel>


<!-- Customize skin -->

<asp:Panel ID="pl_customizeDesign" Runat="server" Visible="True">


<div id="customizeSkin">
<asp:PlaceHolder ID="ph_customizeDesig" runat="server" />
</div>

</asp:Panel>

