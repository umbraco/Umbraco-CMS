<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="rsd.aspx.cs" Inherits="umbraco.presentation.umbraco.channels.rsd" %><?xml version="1.0" encoding="UTF-8"?><rsd version="1.0" xmlns="http://archipelago.phrasewise.com/rsd">
  <service>
    <engineName>umbraco</engineName>
    <engineLink>http://umbraco.org/</engineLink>
    <homePageLink>http://<%=Request.ServerVariables["SERVER_NAME"]%></homePageLink>
    <apis>
      <api name="MetaWeblog" blogID="1" preferred="true" apiLink="http://<%=Request.ServerVariables["SERVER_NAME"]%><%=umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) %>/channels.aspx" />
      <api name="Blogger" blogID="1" preferred="false" apiLink="http://<%=Request.ServerVariables["SERVER_NAME"]%><%=umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) %>/channels.aspx" />
    </apis>
  </service>
</rsd>
