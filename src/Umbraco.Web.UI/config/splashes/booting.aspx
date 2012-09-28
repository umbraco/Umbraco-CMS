<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.Page" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>The website is restarting</title>
    <META HTTP-EQUIV=REFRESH CONTENT="10; URL=<%=Request["url"] %>">
</head>
<body>
    <h1>The website is restarting</h1>
    <p>Please wait for 10s while we prepare to serve the page you have requested...</p>
    
    <p style="border-top: 1px solid #ccc; padding-top: 10px;">
        <small>You can modify the design of this page by editing /config/splashes/booting.aspx</small>
    </p>

</body>
</html>
