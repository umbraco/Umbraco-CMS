<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.Page" %>
<%
    // NH: Adds this inline check to avoid a simple codebehind file in the legacy project!
    var url = Request["url"];
    var isUrlLocal = System.Web.WebPages.RequestExtensions.IsUrlLocalToHost(null, url);
    if (url.ToLower().Contains("booting.aspx") || isUrlLocal == false)
    {
        throw new ArgumentException("Can't redirect to the requested url - it's not local", "url");
    }
%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>The website is restarting</title>
    <meta http-equiv="REFRESH" content="10; URL=<%=url %>">
</head>
<body>
    <h1>The website is restarting</h1>
    <p>Please wait for 10s while we prepare to serve the page you have requested...</p>

    <p style="border-top: 1px solid #ccc; padding-top: 10px;">
        <small>You can modify the design of this page by editing /config/splashes/booting.aspx</small>
    </p>

</body>
</html>
