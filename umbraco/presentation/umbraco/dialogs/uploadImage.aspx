<%@ Page language="c#"MasterPageFile="../masterpages/umbracoDialog.Master" Codebehind="uploadImage.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.uploadImage" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

		<script type="text/javascript" language="javascript">
		
			function validateImage() {
				// Disable save button
			  var imageTypes = ",jpeg,jpg,gif,bmp,png,tiff,tif,";
        var tb_title = document.getElementById("ctl00_body_TextBoxTitle");
        var bt_submit = document.getElementById("ctl00_body_Button1");
        var tb_image = document.getElementById("ctl00_body_uploadFile");
        
        bt_submit.disabled = true;

        var imageName = tb_image.value;
				if (imageName.length > 0) {
				    var extension = imageName.substring(imageName.lastIndexOf(".") + 1, imageName.length);
				    if (imageTypes.indexOf(',' + extension.toLowerCase() + ',') > -1) {
				        bt_submit.disabled = false;
				        if(tb_title.value == "")
				          tb_title.value = imageName.substring(imageName.lastIndexOf("\\") + 1, imageName.length).replace("." + extension, "");
					}
				}
			}
			
			function chooseId() {
				var treePicker = window.showModalDialog('<%=umbraco.cms.presentation.Trees.TreeService.GetPickerUrl(true,"media","media")%>', 'treePicker', 'dialogWidth=350px;dialogHeight=300px;scrollbars=no;center=yes;border=thin;help=no;status=no')			
				if (treePicker != undefined) {
				    document.getElementById("ctl00_body_folderid").value = treePicker;
					if (treePicker > 0) {
						umbraco.presentation.webservices.CMSNode.GetNodeName('<%=umbraco.BasePages.BasePage.umbracoUserContextID%>', treePicker, updateContentTitle);
					}				
				}
			}			
			function updateContentTitle(result) {
				document.getElementById("mediaTitle").innerHTML = "<strong>" + result + "</strong>";
			}
		</script>
		
		<style type="text/css">
		  body, html{margin: 0px !Important; padding: 0px !Important;}
		</style>
		
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
			<cc1:Pane id="pane_upload" runat="server">
				    <cc1:PropertyPanel ID="pp_name" runat="server" Text="Name">
				        <asp:TextBox id="TextBoxTitle" runat="server"></asp:TextBox>
				    </cc1:PropertyPanel>
				    <cc1:PropertyPanel ID="pp_file" runat="server" Text="File">
				        <asp:PlaceHolder id="PlaceHolder1" runat="server"></asp:PlaceHolder>
				    </cc1:PropertyPanel>
				    <cc1:PropertyPanel ID="pp_target" runat="server" Text="Save at...">
				        <asp:PlaceHolder ID="mediaPicker" runat="server"></asp:PlaceHolder>	
				    </cc1:PropertyPanel>
				    <cc1:PropertyPanel ID="pp_button" runat="server" Text=" ">
				        <asp:Button id="Button1" runat="server" Enabled="False" Text="Button" onclick="Button1_Click"></asp:Button>
				    </cc1:PropertyPanel>
		  </cc1:Pane>
		  
		  <cc1:Feedback ID="feedback" runat="server" />
</asp:Content>
  