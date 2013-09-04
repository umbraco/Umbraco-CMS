<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Analyzer.aspx.cs" Inherits="Umbraco.Forms.UI.Pages.Analyzer" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:DataGrid ID="grid" runat="server" AutoGenerateColumns="true" GridLines="Both" />
        
        <asp:DataGrid ID="grid2" runat="server" AutoGenerateColumns="true" GridLines="Both" />

    </div>
    </form>
</body>
</html>
