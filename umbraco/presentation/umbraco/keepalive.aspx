<%@ Page Language="c#" Codebehind="keepalive.aspx.cs" AutoEventWireup="True" Inherits="umbraco.presentation.keepalive" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
  <title>keepalive</title>
  <meta http-equiv="Refresh" content="180; URL=keepalive.aspx">
</head>
<body >
<script type="text/javascript">
  if (top.location == location) {
      top.location.href = "umbraco.aspx";
  }
</script>
  <form id="Form1" method="post" runat="server">
  </form>
</body>
</html>
