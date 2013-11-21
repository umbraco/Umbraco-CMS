<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserControlProxy.aspx.cs" Inherits="Umbraco.Web.UI.Umbraco.Dashboard.UserControlProxy" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

   <cc1:UmbracoClientDependencyLoader runat="server" ID="ClientLoader" />

   <umb:CssInclude ID="CssInclude1" runat="server" FilePath="assets/css/umbraco.css" PathNameAlias="UmbracoRoot" />
    
   <umb:JsInclude ID="JsInclude1" runat="server" FilePath="Application/NamespaceManager.js" PathNameAlias="UmbracoClient" Priority="0" />
   <umb:JsInclude ID="JsInclude3" runat="server" FilePath="ui/jquery.js" PathNameAlias="UmbracoClient" Priority="1" />
   <umb:JsInclude ID="JsInclude6" runat="server" FilePath="ui/base2.js" PathNameAlias="UmbracoClient" Priority="1" />
   <umb:JsInclude ID="JsInclude11" runat="server" FilePath="UI/knockout.js" PathNameAlias="UmbracoClient" Priority="3" />
   <umb:JsInclude ID="JsInclude12" runat="server" FilePath="UI/knockout.mapping.js" PathNameAlias="UmbracoClient" Priority="4" />
 
    <umb:JsInclude ID="JsInclude2" runat="server" FilePath="ui/default.js" PathNameAlias="UmbracoClient" Priority="5" />

</head>
<body style="overflow: scroll">
    <form id="form1" runat="server">
    <div>
            <asp:PlaceHolder ID="container" runat="server" />    
    </div>
    </form>
</body>
</html>
