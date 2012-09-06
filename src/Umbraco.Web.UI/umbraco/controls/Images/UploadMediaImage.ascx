<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UploadMediaImage.ascx.cs"
    Inherits="umbraco.controls.Images.UploadMediaImage" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="ctl" Namespace="umbraco.controls" Assembly="umbraco" %>

<umb:JsInclude ID="JsInclude1" runat="server" FilePath="controls/Images/UploadMediaImage.js" PathNameAlias="UmbracoRoot" />

<script type="text/javascript">
    var uploader_<%=this.ClientID%> = new Umbraco.Controls.UploadMediaImage("<%=TextBoxTitle.ClientID%>", "<%=SubmitButton.ClientID%>", "<%=((Control)UploadField.DataEditor).ClientID%>");
</script>

<cc1:pane id="pane_upload" runat="server">
    <cc1:PropertyPanel ID="pp_name" runat="server" Text="Name">
        <asp:TextBox id="TextBoxTitle" runat="server"></asp:TextBox>
    </cc1:PropertyPanel>
    <cc1:PropertyPanel ID="pp_file" runat="server" Text="File">
        <asp:PlaceHolder id="UploadControl" runat="server"></asp:PlaceHolder>
    </cc1:PropertyPanel>
    <cc1:PropertyPanel ID="pp_target" runat="server" Text="Save at...">
        <ctl:ContentPicker runat="server" ID="MediaPickerControl" AppAlias="media" TreeAlias="media" 
            ModalHeight="200" ShowDelete="false" ShowHeader="false" Text='<%#umbraco.BasePages.BasePage.Current.getUser().StartMediaId.ToString()%>' />        
    </cc1:PropertyPanel>
    <cc1:PropertyPanel ID="pp_button" runat="server" Text=" ">
        <asp:Button id="SubmitButton" runat="server" Text='<%#umbraco.ui.Text("save")%>' Enabled="false" OnClick="SubmitButton_Click"></asp:Button>
    </cc1:PropertyPanel>
</cc1:pane>
<cc1:feedback id="feedback" runat="server" />
