<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="booting.aspx.cs" Inherits="umbraco.presentation.config.splashes.booting" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Website booting</title>
    <META HTTP-EQUIV=REFRESH CONTENT="10; URL=<%=Request["orgUrl"] %>">
</head>
<body>
    <h1>Website is restarting</h1>
    <p>The webpage cannot be displayed right now.</p>
    <p>This page will refresh in ten seconds.</p>
    
    <div style="border-top: 1px solid #999; padding-top: 5px">
        <p>You can modify the design of this page by editing /config/splashes/booting.aspx</p>
    </div>

</body>
</html>
