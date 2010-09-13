<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImageUploader.aspx.cs" Inherits="umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule.ImageUploader" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

    <script type="text/javascript" src="../../../../umbraco_client/ui/jquery.js"></script>

    <script type="text/javascript">

        function setImage() {

            var val = $('#<%= Image.ClientID %>').val();
            top.jQuery('#<%= Request["ctrl"] %>').val(val);
            top.jQuery('#<%= Request["ctrl"] %>').trigger('change');
            closeModal();
        }

        function closeModal() {

            return false;
        }
    
    </script>
</head>
<body>
    <form id="form1" runat="server">


    <asp:HiddenField ID="Image" runat="server" />

    <asp:FileUpload ID="FileUpload1" runat="server" /> 
    <asp:Button ID="bt_upload" runat="server" Text="Upload" 
        onclick="bt_upload_Click" /><br />

    <asp:Image ID="Image1" runat="server" />


    <br />

    <button type="button" id="bt_select" onclick=" setImage();">Select</button> or
    <button type="button" onclick="closeModal();">Cancel</button>

    </form>
</body>
</html>
