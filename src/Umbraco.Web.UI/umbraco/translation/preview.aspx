<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../masterpages/umbracoPage.Master" CodeBehind="preview.aspx.cs" Inherits="umbraco.presentation.translation.preview" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="body" runat="server">

<table cellpadding="0" cellspacing="0" style="text-align: center;">
<tr>
<td><h2>Translated version</h2></td>
<td><h2>Original</h2></td>
</tr>
<tr>
  <td colspan="2">
      <div class="notice">
        <p>
            <strong>Please notice</strong> that due to templating and unpublished content, the 2 pages can have differences in layout. 
        </p>
      </div>
  </td>
</tr>
<tr>
  <td style="border-right: 1px solid #ccc; padding-right: 15px;">
 
  <iframe src="<%= translatedUrl %>" frameborder="0" style="border: none"></iframe>
  </td>
  <td style="padding-left: 15px;">
 
  <iframe src="<%= originalUrl %>" frameborder="0" style="border: none"></iframe>
  </td>
</tr>
</table>

<script type="text/javascript">
  jQuery(document).ready(function() {
    var docHeight = jQuery(document).height();
    var docWidth = jQuery(document).width();

    jQuery("iframe").height(docHeight - 140).width((docWidth / 2) - 31);
  });     
</script>

</asp:Content>