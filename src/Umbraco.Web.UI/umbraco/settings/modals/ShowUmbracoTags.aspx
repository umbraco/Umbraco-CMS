<%@ Page Language="c#" MasterPageFile="../../masterpages/umbracoPage.Master" Title="ShowUmbracoTags"
    Codebehind="ShowUmbracoTags.aspx.cs" AutoEventWireup="True"
  Inherits="umbraco.cms.presentation.settings.modal.ShowUmbracoTags" %>
<%@ Import Namespace="Umbraco.Core.IO" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
    code{display: block; background: #999; color: #fff; padding: 5px; margin-bottom: 10px;white-space:normal;text-wrap:normal;}
    small{color: #000 !Important; margin-bottom: 10px; display: block;}
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <cc1:Pane ID="Pane7" Style="padding-right: 10px; padding-left: 10px; padding-bottom: 10px;
            padding-top: 10px; text-align: left" runat="server">
            <table id="Table1" width="100%">
              <tr><th width="120">Insert field</th><td>
              <code>
                &lt;umbraco:Item field="bodyText" runat="server"/&gt;
              </code>
              <small>
              Fetches a value from the current page.
              </small>
              </td></tr>
              <tr><th width="120">Insert macro</th><td>
              <code>
              &lt;umbraco:Macro macroAlias="MacroAlias" Alias="MacroAlias" runat="server"/&gt;
              </code>
              <small>Inserts a macro into the template</small>
              </td></tr>
              <tr><th width="120">Load child template</th><td>
              <code>
                &lt;asp:ContentPlaceHolder runat="server" id="<%= alias %>ContentPlaceHolder" /&gt;
              </code>
              <small>
              This is the default placeholder for content stored in a child template using this exact template as it's master template.
              </small>
              </td></tr>
              <tr><th width="120">Disable Request Validation</th><td>
              <code>
              &lt;umbraco:DisableRequestValidation runat="server"/&gt;
              </code>
              <small>Disable ASP.NET request validation. It's the same as adding a enableEventValidation="false" to a page directive (but this is not possible in Umbraco as all pages use the same ASPX page for all pages)</small>
              </td></tr>
              <tr><th width="120">MetaBlogApi / Content Channels</th><td>
              <code>
              &lt;link rel="EditURI" type="application/rsd+xml" href="http://<%=Request.ServerVariables["SERVER_NAME"] %><%= IOHelper.ResolveUrl(SystemDirectories.Umbraco)%>/channels/rsd.aspx" /&gt;
              <br /><br />
              &lt;link rel="wlwmanifest" type="application/wlwmanifest+xml" href="http://<%=Request.ServerVariables["SERVER_NAME"] %><%= IOHelper.ResolveUrl(SystemDirectories.Umbraco)%>/channels/wlwmanifest.aspx" /&gt;
              </code>
              <small>
              Insert the above two elements to the head element to gain optimal support for
              using the MetaBlog Apis with 3rd party clients and to enable autodiscovery for Windows
              Live Writer.
              </small>
              </td></tr>
            </table>
    </cc1:Pane>
</asp:Content>
