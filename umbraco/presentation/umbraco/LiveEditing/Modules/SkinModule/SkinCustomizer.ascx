<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SkinCustomizer.ascx.cs" Inherits="umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule.SkinCustomizer" %>


<div id="costumizeSkin" <asp:Literal ID="ltCustomizeSkinStyle" runat="server" Text=""></asp:Literal>>

    <div id="dependencies">
        <asp:PlaceHolder ID="ph_dependencies" runat="server"></asp:PlaceHolder>
    </div>

    <p runat="server" id="pChangeSkin">... or <a href="#" onclick="jQuery('#costumizeSkin').hide();jQuery('#changeSkin').show();">change</a> skin</p>

    <asp:Button ID="btnOk" runat="server" Text="Ok" onclick="btnOk_Click" />
    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="modalbuton" />

</div>

<div id="changeSkin" <asp:Literal ID="ltChangeSkinStyle" runat="server" Text="style='display:none;'"></asp:Literal>>
    
    <div id="skins">
         <asp:PlaceHolder ID="ph_skins" runat="server"></asp:PlaceHolder>
    </div>

    <p runat="server" id="pCustomizeSkin">... or <a href="#" onclick="jQuery('#changeSkin').hide();jQuery('#costumizeSkin').show();">customize</a> current skin</p>

    <asp:Button ID="btnCancelSkin" runat="server" Text="Cancel" CssClass="modalbuton" />

</div>




