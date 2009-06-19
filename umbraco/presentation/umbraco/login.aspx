<%@ Page Language="c#" Codebehind="login.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.login" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register Namespace="umbraco" TagPrefix="umb" Assembly="umbraco" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" >
<html style="width: 100%; height: 100%">
<head>
  <title>Umbraco - login</title>
  <link href="/umbraco_client/ui/default.css" type="text/css" rel="stylesheet"/>
  <script type="text/javascript" src="/umbraco_client/ui/default.js"></script>
  
  <style type="text/css">
  body{font-size: 11px; width: 100%; font-family: Trebuchet MS, verdana, arial, Lucida Grande;
  text-align: center; padding-top: 50px; margin: 0px;}
  
  #Panel1_content{background: url(images/loginBg.gif) no-repeat 1px 5px}
  #Panel1_innerContent{width: auto; height: auto; padding: 0px !Important;}
  
  label{padding-right: 20px;}
  .copyright{padding: 10px 0px 0px 0px; font-size: 11px;}
  .copyright a{text-decoration: underline !Important; padding-left: 5px;}
  </style>
  
  
  
</head>
<body>
  <form id="Form1" method="post" runat="server">
   
    <cc1:UmbracoPanel Style="text-align: left;" ID="Panel1" runat="server" Height="347px" Width="340px" Text="Umbraco 4 login" AutoResize="false">
      
      <div style="padding: 70px 0px 0px 0px;">
      
        <p style="margin: 0px; padding: 5px 0px 20px 0px; color: #999">
          <asp:Literal ID="TopText" runat="server"></asp:Literal>
        </p>
        
        <table cellspacing="0" cellpadding="0" border="0">
          <tr>
            <td align="right">
            <asp:Label ID="username" runat="server" AssociatedControlID="lname"></asp:Label>
            </td>
            
            <td>
              <asp:TextBox ID="lname" Style="padding-left: 3px; background: url(images/gradientBackground.png);
                _background: none; border-right: #999999 1px solid; border-top: #999999 1px solid;
                border-left: #999999 1px solid; border-bottom: #999999 1px solid; width: 180px;"
                runat="server"></asp:TextBox></td>
          </tr>
          <tr>
            <td colspan="2" style="height: 12px;">
            </td>
          </tr>
          <tr>
            <td align="right">
            <asp:Label ID="password" runat="server" AssociatedControlID="passw"></asp:Label>
            </td>
            <td>
              <asp:TextBox ID="passw" Style="padding-left: 3px; background: url(images/gradientBackground.png);
                _background: none; border-right: #999999 1px solid; border-top: #999999 1px solid;
                border-left: #999999 1px solid; border-bottom: #999999 1px solid; width: 180px;"
                runat="server" TextMode="Password"></asp:TextBox></td>
          </tr>
          <tr>
            <td colspan="2" style="height: 12px;">
            </td>
          </tr>
          <tr>
            <td align="right" colspan="2">
              <asp:Button ID="Button1" Style="width: 60px; font-weight: bold" Text="" runat="server"
                OnClick="Button1_Click"></asp:Button></td>
          </tr>
        </table>
        
              
      </div>
    </cc1:UmbracoPanel>
    
    <small class="copyright"><asp:Literal ID="BottomText" runat="server"></asp:Literal></small>  
    
    <asp:HiddenField id="hf_height" runat="server" />
    <asp:HiddenField ID="hf_width" runat="server" />
  </form>

  <script type="text/javascript">
			document.getElementById("lname").focus();
      document.getElementById('<%= hf_height.ClientID %>').value = getViewportHeight();
      document.getElementById('<%= hf_width.ClientID %>').value = getViewportWidth();    
  </script>
  

</body>
</html>
