<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SkinCustomizer.ascx.cs" Inherits="umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule.SkinCustomizer" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>


<script type="text/javascript">

    function closeCustomizeSkinModal() {

        jQuery('#cancelSkinCustomization').trigger('click');
        UmbSpeechBubble.ShowMessage("Info", "Skin", "Skin updated...");
    }

</script>


<div id="costumizeSkin" <asp:Literal ID="ltCustomizeSkinStyle" runat="server" Text=""></asp:Literal>>

    <div id="dependencies">
        <asp:PlaceHolder ID="ph_dependencies" runat="server"></asp:PlaceHolder>
    </div>

    <p runat="server" id="pChangeSkin">... or <a href="#" onclick="jQuery('#costumizeSkin').hide();jQuery('#changeSkin').show();">change</a> skin</p>

    <asp:Button ID="btnOk" runat="server" Text="Ok" onclick="btnOk_Click" OnClientClick="closeCustomizeSkinModal();"/>

    <button type="button" class="modalbuton" id="cancelSkinCustomization">Cancel</button>

</div>

<div id="changeSkin" <asp:Literal ID="ltChangeSkinStyle" runat="server" Text="style='display:none;'"></asp:Literal>>
    
    <div id="skins">
        <asp:Repeater ID="rep_starterKitDesigns" runat="server">
            <HeaderTemplate>
                <ul id="starterKitDesigns">
            </HeaderTemplate>
            <ItemTemplate>
                <li>
                  
                   <img src="<%# ((Skin)Container.DataItem).Thumbnail %>" alt="<%# ((Skin)Container.DataItem).Text %>" />
        
                   <span><%# ((Skin)Container.DataItem).Text %></span>

                   <br />
                       
                    <asp:Button ID="Button1" runat="server" Text="Install" CommandArgument="<%# ((Skin)Container.DataItem).RepoGuid %>" OnClick="SelectStarterKitDesign"/>
                </li>
            </ItemTemplate>
            
            <FooterTemplate>
                </ul>
            </FooterTemplate>
        </asp:Repeater>

    </div>

    <p runat="server" id="pCustomizeSkin">... or <a href="#" onclick="jQuery('#changeSkin').hide();jQuery('#costumizeSkin').show();">customize</a> current skin</p>

    <button type="button" class="modalbuton">Cancel</button>

</div>




