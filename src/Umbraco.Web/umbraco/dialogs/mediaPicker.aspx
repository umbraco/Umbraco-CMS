<%@ Page Language="C#" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="true"
    CodeBehind="mediaPicker.aspx.cs" Inherits="umbraco.presentation.umbraco.dialogs.mediaPicker" %>

<%@ Register TagPrefix="ui" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="umb2" TagName="Tree" Src="../controls/Tree/TreeControl.ascx" %>
<%@ Register TagPrefix="umb3" TagName="Image" Src="../controls/Images/ImageViewer.ascx" %>
<%@ Register TagName="MediaUpload" TagPrefix="umb4" Src="../controls/Images/UploadMediaImage.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <script type="text/javascript">

        //need to wire up the submit button click
        jQuery(document).ready(function() {
            jQuery("#submitbutton").click(function() {
                updatePicker();
                return false;
            });
        });

        //called when the user interacts with a node
        function dialogHandler(id) {
            if (id != -1) {
                //update the hidden field with the selected id
                jQuery("#selectedMediaId").val(id);
                jQuery("#submitbutton").removeAttr("disabled").css("color", "#000");
            }
            else {
                jQuery("#submitbutton").attr("disabled", "disabled").css("color", "gray");
            }

            jQuery("#<%=ImageViewer.ClientID%>").UmbracoImageViewerAPI().updateImage(id, function(p) {
                //when the image is loaded, this callback method fires
                if (p.hasImage) {
                    jQuery("#submitbutton").removeAttr("disabled").css("color", "#000");                    
                }
                else {
                    jQuery("#submitbutton").attr("disabled", "disabled").css("color", "gray");
                }
            });
        }

        function uploadHandler(e) {
            dialogHandler(e.id);
            //get the tree object for the chooser and refresh
            var tree = jQuery("#<%=DialogTree.ClientID%>").UmbracoTreeAPI();
            tree.refreshTree();
        }

        function updatePicker() {
            var id = jQuery("#selectedMediaId").val();
            if (id != "") {
                UmbClientMgr.closeModalWindow(id);
            }
        }

        function cancel() {
            //update the hidden field with the selected id = none
            jQuery("#selectedMediaId").val("");
            UmbClientMgr.closeModalWindow();
        }

    </script>

    <style type="text/css">        
        .imageViewer .bgImage {float:right; }
    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <%--when a node is selected, the id will be stored in this field--%>
    <input type="hidden" id="selectedMediaId" />
    <ui:Pane ID="pane_src" runat="server">
        <umb3:Image runat="server" ID="ImageViewer" ViewerStyle="ThumbnailPreview" />
    </ui:Pane>
    <br />
    <ui:TabView AutoResize="false" Width="455px" Height="305px" runat="server" ID="tv_options" />
    <ui:Pane ID="pane_select" runat="server">
        <umb2:Tree runat="server" ID="DialogTree" App="media" TreeType="media" IsDialog="true"
            ShowContextMenu="false" DialogMode="id" FunctionToCall="dialogHandler"
            Height="250"/>
    </ui:Pane>
    <asp:Panel ID="pane_upload" runat="server">
        <umb4:MediaUpload runat="server" ID="MediaUploader" OnClientUpload="uploadHandler" />
    </asp:Panel>
    <br />
    <p>
        <input type="submit" value="<%# umbraco.ui.Text("treepicker")%>" style="width: 60px;
            color: gray" disabled="disabled" id="submitbutton" />
        <em id="orcopy">
            <%# umbraco.ui.Text("or") %></em> <a href="javascript:cancel();" style="color: blue"
                id="cancelbutton">
                <%#umbraco.ui.Text("cancel") %></a>
    </p>
</asp:Content>
