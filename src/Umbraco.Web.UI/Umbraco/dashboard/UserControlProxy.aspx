<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserControlProxy.aspx.cs" Inherits="Umbraco.Web.UI.Umbraco.Dashboard.UserControlProxy" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="cc1" Namespace="Umbraco.Web.UI.JavaScript" Assembly="umbraco" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

   <cc1:UmbracoClientDependencyLoader runat="server" ID="ClientLoader" />

   <umb:CssInclude ID="CssInclude1" runat="server" FilePath="assets/css/umbraco.css" PathNameAlias="UmbracoRoot" />
    
   <umb:JsInclude ID="JsInclude1" runat="server" FilePath="Application/NamespaceManager.js" PathNameAlias="UmbracoClient" Priority="0" />
   <umb:JsInclude ID="JsInclude4" runat="server" FilePath="lib/jquery-migrate/jquery-migrate.min.js" PathNameAlias="UmbracoRoot" Priority="1" />

</head>
<body style="overflow: scroll">
    <form id="form1" runat="server">
    <div>
            <asp:PlaceHolder ID="container" runat="server" />    
    </div>
    </form>
</body>
</html>
