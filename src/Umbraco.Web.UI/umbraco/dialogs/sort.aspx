<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" CodeBehind="sort.aspx.cs" AutoEventWireup="True" Inherits="Umbraco.Web.UI.Umbraco.Dialogs.Sort" %>

<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="Umbraco.Core.IO" %>
<%@ Import Namespace="Umbraco.Web" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    <umb:CssInclude ID="CssInclude1" runat="server" FilePath="Dialogs/SortDialog.css" PathNameAlias="UmbracoClient"></umb:CssInclude>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">

    <umb:JsInclude ID="JsInclude3" runat="server" FilePath="Dialogs/SortDialog.js" PathNameAlias="UmbracoClient" />
    <umb:JsInclude ID="JsInclude1" runat="server" FilePath="tablesorting/jquery.tablesorter.min.js" PathNameAlias="UmbracoClient" />
    <umb:JsInclude ID="JsInclude2" runat="server" FilePath="tablesorting/tableDragAndDrop.js" PathNameAlias="UmbracoClient" />

    <div class="umb-dialog-body">
        <cc1:Pane runat="server">

          <div id="loading" style="display: none;">
                <div class="notice">
                    <p><%= umbraco.ui.Text("sort", "sortPleaseWait") %></p>
                </div>
                <br />
                <cc1:ProgressBar ID="prog1" runat="server" Title="sorting.." />
            </div>

            <div id="sortingDone" style="display: none;" class="success">
                <p>
                    <asp:Literal runat="server" ID="sortDone"></asp:Literal>
                </p>
                <p>
                    <a href="#" onclick="UmbClientMgr.closeModalWindow()"><%= umbraco.ui.Text("defaultdialogs", "closeThisWindow")%></a>
                </p>
            </div>

            <div id="sortArea">
                <p class="help">
                    <%= umbraco.ui.Text("sort", "sortHelp") %>
                </p>

                <div id="sortableFrame">
                    <table id="sortableNodes">
                        <thead>
                            <tr>
                                <th style="width: 100%">Name</th>
                                <th class="nowrap">Creation date</th>
                                <th class="nowrap">Sort order</th>
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
        <a id="closeWindowButton" href="#" class="btn btn-link"><%=umbraco.ui.Text("general", "cancel", this.getUser())%></a>
        <input id="submitButton" type="button" class="btn btn-primary" value="<%=umbraco.ui.Text("save") %>" />
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
