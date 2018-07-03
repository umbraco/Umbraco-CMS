<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" CodeBehind="sort.aspx.cs" AutoEventWireup="True" Inherits="Umbraco.Web.UI.Umbraco.Dialogs.Sort" %>

<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="Umbraco.Core.IO" %>
<%@ Import Namespace="Umbraco.Web" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="cc1" Namespace="Umbraco.Web._Legacy.Controls" Assembly="Umbraco.Web" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    <umb:CssInclude ID="CssInclude1" runat="server" FilePath="Dialogs/SortDialog.css" PathNameAlias="UmbracoClient"></umb:CssInclude>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">

    <umb:JsInclude ID="JsInclude3" runat="server" FilePath="Dialogs/SortDialog.js" PathNameAlias="UmbracoClient" />
    <umb:JsInclude ID="JsInclude1" runat="server" FilePath="tablesorting/jquery.tablesorter.min.js" PathNameAlias="UmbracoClient" />
    <umb:JsInclude ID="JsInclude2" runat="server" FilePath="tablesorting/tableDragAndDrop.js" PathNameAlias="UmbracoClient" />

    <div class="umb-dialog-body">
        <cc1:Pane runat="server">

          <div id="loading" style="display: none; margin-bottom: 35px;">
                <div class="notice">
                    <p><%= Services.TextService.Localize("sort/sortPleaseWait") %></p>
                </div>

                <div class="umb-loader-wrapper">
                    <cc1:ProgressBar ID="prog1" runat="server" Title="sorting.." />
                </div>
            </div>

            <div id="sortingDone" style="display: none;" class="success">
                <p>
                    <asp:Literal runat="server" ID="sortDone"></asp:Literal>
                </p>
                <p>
                    <a href="#" onclick="UmbClientMgr.closeModalWindow()"><%= Services.TextService.Localize("defaultdialogs/closeThisWindow")%></a>
                </p>
            </div>

            <div id="sortArea">
                <p class="help">
                    <%= Services.TextService.Localize("sort/sortHelp") %>
                </p>

                <div id="sortableFrame">
                    <table id="sortableNodes">
                        <thead>
                            <tr>
                                <th style="width: 100%"><%= Services.TextService.Localize("general/name") %></th>
                                <th class="nowrap" style="display: <%= HideDateColumn ? "none" : "table-cell" %>;"><%= Services.TextService.Localize("sort/sortCreationDate") %></th>
                                <th class="nowrap"><%= Services.TextService.Localize("sort/sortOrder") %></th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Literal ID="lt_nodes" runat="server" />
                        </tbody>
                    </table>
                </div>
            </div>
        </cc1:Pane>

    </div>
    <div class="umb-dialog-footer btn-toolbar umb-btn-toolbar">
        <a id="closeWindowButton" href="#" class="btn btn-link"><%=Services.TextService.Localize("general/cancel")%></a>
        <input id="submitButton" type="button" class="btn btn-primary" value="<%=Services.TextService.Localize("save") %>" />
    </div>

    <script type="text/javascript">

        jQuery(document).ready(function () {

            var sortDialog = new Umbraco.Dialogs.SortDialog({
                submitButton: jQuery("#submitButton"),
                closeWindowButton: jQuery("#closeWindowButton"),
                dateTimeFormat: "<%=CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern%> <%=CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern%>",
                currentId: "<%=Request.CleanForXss("ID")%>",
                serviceUrl: "<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco)%>/WebServices/NodeSorter.asmx/UpdateSortOrder?app=<%=Request.CleanForXss("app")%>"
            });

            sortDialog.init();
        });

    </script>

</asp:Content>