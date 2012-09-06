<%@ Page Language="C#" Title="Submit package" MasterPageFile="../../masterpages/umbracoPage.Master" AutoEventWireup="true" CodeBehind="SubmitPackage.aspx.cs" Inherits="umbraco.presentation.developer.packages.SubmitPackage" %>
<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="footer" runat="server">
<script type="text/javascript">
  var tb_email = document.getElementById('<%= tb_email.ClientID %>');
 
  if (tb_email.value != "") {
    onRepoChange();
  }
</script>
</asp:Content>

<asp:Content ContentPlaceHolderID="head" runat="server">
<script type="text/javascript">
  function onRepoChange() {

    var dropdown = document.getElementById('<%= dd_repositories.ClientID %>');
    var myindex = dropdown.selectedIndex
    var SelValue = dropdown.options[myindex].value
    var repoLogin = document.getElementById('<%= pl_repoLogin.ClientID %>');

    if (SelValue != "") {

      var publicRepoHelp = document.getElementById('<%= publicRepoHelp.ClientID %>');
      var privateRepoHelp = document.getElementById('<%= privateRepoHelp.ClientID %>');

      publicRepoHelp.style.display = 'none';
      privateRepoHelp.style.display = 'none';

      if (SelValue == "65194810-1f85-11dd-bd0b-0800200c9a66") {
        publicRepoHelp.style.display = 'block';
      } else {
        privateRepoHelp.style.display = 'block';
      }

      repoLogin.style.display = 'block';

    } else {
      repoLogin.style.display = 'none';
    }
  }
  </script>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
<cc2:UmbracoPanel ID="Panel1" Text="Submit package to repository" runat="server" Width="496px" Height="584px">
    <br />
    <cc2:Feedback ID="fb_feedback" runat="server" />
    <asp:PlaceHolder ID="feedbackControls" runat="server" Visible="false">
      <br />
      <p>
      <button onclick="window.location.href = 'editpackage.aspx?id=<%= Request.QueryString["id"] %>'; return false;">Ok</button>
      </p>
    </asp:PlaceHolder>
    
    <cc2:Pane ID="Pane2" runat="server" Text="Repository">
      
      <asp:Panel ID="pl_repoChoose" runat="server">
        <cc2:PropertyPanel runat="server">
            <p>Choose the repository you want to submit the package to</p>
        </cc2:PropertyPanel>
        <cc2:PropertyPanel Text="Repository" runat="server">
            <asp:DropDownList ID="dd_repositories" runat="server" />
        </cc2:PropertyPanel>
      </asp:Panel>
      
      <asp:Panel id="pl_repoLogin" style="display: none;" runat="server">
      <cc2:PropertyPanel ID="PropertyPanel1" runat="server">
      
      <h3 style="margin-left: 0px; padding-top: 15px;">Please enter your credentials to authenticate your user.</h3>
      <p runat="server" id="publicRepoHelp" style="display: none">If you do not have a user on the umbraco package repository, you can create one <a href="http://packages.umbraco.org/create-user" target="_blank">here</a>.</p>
      <p runat="server" id="privateRepoHelp" style="display: none">If you do not have a user on this private repository, contact your repository administrator to gain access</p>
      </cc2:PropertyPanel>
      
      <cc2:PropertyPanel ID="PropertyPanel2" runat="server" Text="Email">
          <asp:TextBox ID="tb_email" runat="server" /> <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="tb_email" runat="server" ErrorMessage="*" />
      </cc2:PropertyPanel>
      
      <cc2:PropertyPanel ID="PropertyPanel3" runat="server" Text="Password">
         <asp:TextBox TextMode="Password" ID="tb_password" runat="server" /> <asp:RequiredFieldValidator ControlToValidate="tb_password" runat="server" ErrorMessage="*" />
      </cc2:PropertyPanel>
      </asp:Panel>
      
    </cc2:Pane>
    
    <cc2:Pane ID="Pane1" runat="server" Text="Documentation (.pdf only)">
       <cc2:PropertyPanel ID="PropertyPanel4" runat="server">
       <p>Upload additional documentation for your package to help new users getting started with your package</p>
       </cc2:PropertyPanel>
       
       <cc2:PropertyPanel ID="PropertyPanel5" runat="server" Text="Documentation file">
        <asp:FileUpload ID="fu_doc" runat="server" />
        <asp:RegularExpressionValidator ID="doc_regex" runat="server" ControlToValidate="fu_doc" ValidationExpression="(.*?)\.(pdf|PDF)$" ErrorMessage="Only .pdf files are accepted" />
       </cc2:PropertyPanel>
    </cc2:Pane> 
    
    <asp:PlaceHolder runat="server" ID="submitControls">
    <br />
    
    <div class="notice">
        <p>By clicking "submit package" below you understand that your package will be submitted to a package repository and will in some cases be publicly available to download.</p>
        <p><strong>Please notice: </strong> only packages with complete read-me, author information and install information gets considered for inclusion.</p>
        <p>The package administrators group reservers the right to decline packages based on lack of documentation, poorly written readme and missing author information</p>
    </div>
    
    <p>
      <asp:Button ID="bt_submit" runat="server" Text="Submit package" OnClick="submitPackage" /> &nbsp;<em><%= umbraco.ui.Text("or") %></em> &nbsp;<a href="editpackage.aspx?id=<%= Request.QueryString["id"] %>"><%= umbraco.ui.Text("cancel") %></a>
    </p>
    </asp:PlaceHolder>
    
    </cc2:UmbracoPanel>
</asp:Content>
