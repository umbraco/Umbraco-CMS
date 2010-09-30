<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SkinCustomizer.ascx.cs" Inherits="umbraco.presentation.LiveEditing.Modules.SkinModule.SkinCustomizer" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<script type="text/javascript">
    function closeCustomizeSkinModal() {
        //jQuery('#cancelSkinInstall').trigger('click');
        UmbSpeechBubble.ShowMessage("Info", "Skin", "Skin updated...");
    }
</script>

<asp:Panel ID="pnl_connectionerror" runat="server" Visible="false">
<p>Connection to repository failed...</p>
</asp:Panel>


<div id="costumizeSkin" <asp:Literal ID="ltCustomizeSkinStyle" runat="server" Text=""></asp:Literal>>

    <div id="dependencies">
        <cc1:Pane ID="ph_dependencies" runat="server" />
    </div>

    <p>
        <asp:Button ID="btnOk" runat="server" Text=" Ok " CssClass="modalButton" onclick="btnOk_Click" OnClientClick="closeCustomizeSkinModal();"/>
        <em> or </em> <a href="#" onclick="closeModal();">Cancel</a>
    </p>



    <p runat="server" id="pChangeSkin">You could also change to another skin <a href="#" onclick="jQuery('#costumizeSkin').hide();jQuery('#changeSkin').show();">change</a> skin</p>

</div>



<div id="changeSkin" <asp:Literal ID="ltChangeSkinStyle" runat="server" Text="style='display:none;'"></asp:Literal>>
    
    <p>
        Choose a skin from your local collection, or download one from the umbraco package repository
    </p>

    <div id="skins">
        <asp:Repeater ID="rep_starterKitDesigns" runat="server" onitemdatabound="rep_starterKitDesigns_ItemDataBound">
            <HeaderTemplate>
                <ul id="starterKitDesigns">
            </HeaderTemplate>
                <ItemTemplate>
                    <li>
                       <img src="<%# ((Skin)Container.DataItem).Thumbnail %>" alt="<%# ((Skin)Container.DataItem).Text %>" />
                       <span><%# ((Skin)Container.DataItem).Text %></span>
                        <br />
                        <asp:LinkButton ID="Button1" runat="server" Text="Install" CommandArgument="<%# ((Skin)Container.DataItem).RepoGuid %>" OnClick="SelectStarterKitDesign" CommandName="<%# ((Skin)Container.DataItem).Text %>"/>
                    </li>
                </ItemTemplate>            
            <FooterTemplate>
                </ul>
            </FooterTemplate>
        </asp:Repeater>

    </div>


    <p runat="server" id="pCustomizeSkin">
        <button onclick="jQuery('#changeSkin').hide(); jQuery('#costumizeSkin').show();">Cancel</button>
    </p>

</div>




