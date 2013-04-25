<?xml version="1.0" encoding="UTF-8" ?>
<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.Page" %>
<%@ Import Namespace="Umbraco.Core.IO" %>
<script runat="server">
protected override void OnInit(EventArgs e)
{
    Response.ContentType = "text/xml";
}
</script>
<rsd version="1.0" xmlns="http://archipelago.phrasewise.com/rsd">
  <service>
    <engineName>umbraco</engineName>
    <engineLink>http://umbraco.org/</engineLink>
    <homePageLink>http://<%=Request.ServerVariables["SERVER_NAME"]%></homePageLink>
    <apis>
      <api name="MetaWeblog" blogID="1" preferred="true" apiLink="http://<%=Request.ServerVariables["SERVER_NAME"]%><%=IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>/channels.aspx" />
      <api name="Blogger" blogID="1" preferred="false" apiLink="http://<%=Request.ServerVariables["SERVER_NAME"]%><%=IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>/channels.aspx" />
    </apis>
  </service>
</rsd>