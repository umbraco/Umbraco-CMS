<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SkinCustomizer.ascx.cs" Inherits="umbraco.presentation.LiveEditing.Modules.SkinModule.SkinCustomizer" %>
<%@ Import Namespace="umbraco.cms.businesslogic.packager.repositories"  %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<style type="text/css">
.skinningslider
{
    width:250px !important;
    height:1px !important;
}

#costumizeSkin input.text, #costumizeSkin input.title, #costumizeSkin textarea, #costumizeSkin select
{
    margin:0;
}
</style>
<script type="text/javascript">
    function closeCustomizeSkinModal() {
        closeSkinModal();
        UmbSpeechBubble.ShowMessage("Info", "Skin", "Skin updated...");
    }

    function closeSkinModal() {
        jQuery('#closeSkinInstall').trigger('click');
    }

    function cancelSkinCustomization() {
        jQuery('#cancelSkinCustomization').trigger('click');
    }
</script>

<asp:Panel ID="pnl_connectionerror" runat="server" Visible="false">
<p>Connection to repository failed...</p>
</asp:Panel>

<!-- Needs to change -->
<input type="submit" class="modalbuton" id="closeSkinInstall" value=""  style="display:none;"/>

<input type="submit" class="modalbuton" id="cancelSkinCustomization" value="" style="display:none;"/>
<!-- Using some hidden controls -->

<div id="costumizeSkin" <asp:Literal ID="ltCustomizeSkinStyle" runat="server" Text=""></asp:Literal>>

    <p>
        Personalize your skin, by defining colors, images and texts
    </p>
    
    <div id="dependencies">
        <cc1:Pane ID="ph_dependencies" runat="server" />
    </div>

    <p style="margin-top: 20px;">
        <asp:Button ID="btnOk" runat="server" Text=" Ok " CssClass="modalButton" onclick="btnOk_Click" OnClientClick="closeCustomizeSkinModal();"/>
        <em> or </em> <a href="#" onclick="cancelSkinCustomization();">Cancel</a>
    </p>


    <p runat="server" id="pChangeSkin" style="margin-top: 25px; border-top: 1px solid #efefef; padding: 7px">You could also change to another skin: <a href="#" onclick="jQuery('#costumizeSkin').hide();jQuery('#changeSkin').show();">Browse available skins</a></p>

</div>



<div id="changeSkin" <asp:Literal ID="ltChangeSkinStyle" runat="server" Text="style='display:none;'"></asp:Literal>>
    
    <p>
        Choose a skin from your local collection, or download one from the umbraco package repository
    </p>

    <div id="skinupdateinprogress" style="display:none;">
    <p>Skin is being updated...</p>
    </div>
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
                        <asp:Button ID="Button1" CssClass="selectskin" runat="server" Text="Install" CommandArgument="<%# ((Skin)Container.DataItem).RepoGuid %>" OnClick="SelectStarterKitDesign" CommandName="<%# ((Skin)Container.DataItem).Text %>"/>
                    </li>
                </ItemTemplate>            
            <FooterTemplate>
                </ul>
            </FooterTemplate>
        </asp:Repeater>

    </div>

    <div id="localSkinsContainer" runat="server">
    <p>Looks like there are also some local skins</p>
    
        <asp:Repeater ID="rep_starterKitDesignsLocal" runat="server"  onitemdatabound="rep_starterKitDesignsLocal_ItemDataBound">
        <HeaderTemplate>
                <ul id="starterKitDesignsLocal">
        </HeaderTemplate>
        <ItemTemplate>
         <li>
                <%# ((string)Container.DataItem).ToString() %>

                <asp:Button ID="btnApply" CssClass="selectskin" runat="server" Text="Apply" CommandArgument="<%# ((string)Container.DataItem).ToString() %>" OnClick="SelectLocalStarterKitDesign"  CommandName="apply"/>
         </li>
        </ItemTemplate>
        <FooterTemplate>
                </ul>
            </FooterTemplate>
        </asp:Repeater>


    </div>

    <p runat="server" id="pCustomizeSkin" style="clear: both; margin-top: 25px; border-top: 1px solid #efefef; padding: 7px" >
        <a onclick="jQuery('#changeSkin').hide(); jQuery('#costumizeSkin').show();">Go back to your current skin</a>
    </p>

</div>




