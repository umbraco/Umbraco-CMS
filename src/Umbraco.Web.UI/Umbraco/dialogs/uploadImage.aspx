<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" CodeBehind="uploadImage.aspx.cs"
    AutoEventWireup="True" Inherits="umbraco.dialogs.uploadImage" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagName="MediaUpload" TagPrefix="umb" Src="../controls/Images/UploadMediaImage.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        body, html
        {
            margin: 0px !important;
            padding: 0px !important;
        }
    </style>

    <script type="text/javascript">
        function uploadHandler(e) {
            //get the tree object for the chooser and refresh
            if (parent && parent.jQuery && parent.jQuery.fn.UmbracoTreeAPI) {
                var tree = parent.jQuery("#treeContainer").UmbracoTreeAPI();
                tree.refreshTree();
            }
        }
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <umb:MediaUpload runat="server" ID="MediaUploader" OnClientUpload="uploadHandler" />
</asp:Content>
