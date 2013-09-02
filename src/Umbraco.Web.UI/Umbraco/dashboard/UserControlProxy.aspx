<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserControlProxy.aspx.cs" Inherits="Umbraco.Web.UI.Umbraco.Dashboard.UserControlProxy" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="../assets/css/umbraco.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
            <asp:PlaceHolder ID="container" runat="server" />    
    </div>
    </form>
</body>
</html>
