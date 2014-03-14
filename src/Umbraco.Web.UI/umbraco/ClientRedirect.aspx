<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.Page" %>
<%--
    This page is required because we cannot reload the angular app with a changed Hash since it just detects the hash and doesn't reload.
    So this is used purely for a full reload of an angular app with a changed hash.
--%>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Redirecting...</title>
    <script type="text/javascript">
        var parts = window.location.href.split("?redirectUrl=");
        if (parts.length != 2) {
            window.location.href = "/";
        }
        else {
            window.location.href = parts[1];
        }
    </script>
</head>
<body>
    <small>Redirecting...</small>
</body>
</html>
