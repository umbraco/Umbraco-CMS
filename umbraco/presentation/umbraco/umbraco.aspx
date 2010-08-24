<%@ Page Trace="false" Language="c#" CodeBehind="umbraco.aspx.cs" AutoEventWireup="True"
    Inherits="umbraco.cms.presentation._umbraco" %>

<%@ Register Src="controls/Tree/TreeControl.ascx" TagName="TreeControl" TagPrefix="umbraco" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="uc1" TagName="quickSearch" Src="Search/QuickSearch.ascx" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Umbraco CMS -
        <%=Request.Url.Host.ToLower().Replace("www.", "") %></title>
    <cc1:UmbracoClientDependencyLoader runat="server" ID="ClientLoader" />
    <umb:CssInclude ID="CssInclude1" runat="server" FilePath="css/umbracoGui.css" PathNameAlias="UmbracoRoot" />
    <umb:CssInclude ID="CssInclude2" runat="server" FilePath="modal/style.css" PathNameAlias="UmbracoClient" />
    <umb:JsInclude ID="JsInclude1" runat="server" FilePath="Application/NamespaceManager.js"
        PathNameAlias="UmbracoClient" Priority="0" />
    <umb:JsInclude ID="JsInclude2" runat="server" FilePath="ui/jquery.js" PathNameAlias="UmbracoClient"
        Priority="0" />
    <umb:JsInclude ID="JsInclude3" runat="server" FilePath="ui/jqueryui.js" PathNameAlias="UmbracoClient"
        Priority="1" />
    <umb:JsInclude ID="JsInclude14" runat="server" FilePath="Application/jQuery/jquery.ba-bbq.min.js"
        PathNameAlias="UmbracoClient" Priority="2" />
    <umb:JsInclude ID="JsInclude5" runat="server" FilePath="Application/UmbracoApplicationActions.js"
        PathNameAlias="UmbracoClient" Priority="2" />
    <umb:JsInclude ID="JsInclude6" runat="server" FilePath="Application/UmbracoUtils.js"
        PathNameAlias="UmbracoClient" Priority="2" />
    <umb:JsInclude ID="JsInclude13" runat="server" FilePath="Application/HistoryManager.js"
        PathNameAlias="UmbracoClient" Priority="3" />
    <umb:JsInclude ID="JsInclude7" runat="server" FilePath="Application/UmbracoClientManager.js"
        PathNameAlias="UmbracoClient" Priority="4" />
    <umb:JsInclude ID="JsInclude8" runat="server" FilePath="ui/default.js" PathNameAlias="UmbracoClient"
        Priority="5" />
    <umb:JsInclude ID="JsInclude9" runat="server" FilePath="ui/jQueryWresize.js" PathNameAlias="UmbracoClient" />
    <umb:JsInclude ID="JsInclude10" runat="server" FilePath="js/guiFunctions.js" PathNameAlias="UmbracoRoot" />
    <umb:JsInclude ID="JsInclude11" runat="server" FilePath="js/language.aspx" PathNameAlias="UmbracoRoot" />
    <umb:JsInclude ID="JsInclude4" runat="server" FilePath="modal/modal.js" PathNameAlias="UmbracoClient"
        Priority="10" />
    <umb:JsInclude ID="JsInclude12" runat="server" FilePath="js/UmbracoSpeechBubbleBackend.js"
        PathNameAlias="UmbracoRoot" />
    <umb:JsInclude ID="JsInclude15" runat="server" FilePath="js/UmbracoSpeechBubbleBackend.js"
        PathNameAlias="UmbracoRoot" />
    <umb:JsInclude ID="JsInclude16" runat="server" FilePath="Application/jQuery/jquery.cookie.js"
        PathNameAlias="UmbracoClient" Priority="1" />
    <script type="text/javascript">
        this.name = 'umbracoMain';
    </script>
</head>
<body id="umbracoMainPageBody">
    <form id="Form1" method="post" runat="server" style="margin: 0px; padding: 0px">
    <asp:ScriptManager runat="server" ID="umbracoScriptManager" ScriptMode="Release">
        <CompositeScript ScriptMode="Release">
            <Scripts>
                <asp:ScriptReference Path="js/dualSelectBox.js" />
            </Scripts>
        </CompositeScript>
        <Services>
            <asp:ServiceReference Path="webservices/legacyAjaxCalls.asmx" />
            <asp:ServiceReference Path="webservices/nodeSorter.asmx" />
        </Services>
    </asp:ScriptManager>
    </form>
    <div style="position: relative;">
        <div class="topBar" id="topBar">
            <div style="float: left">
                <button id="buttonCreate" onclick="UmbClientMgr.appActions().launchCreateWizard();"
                    class="topBarButton" accesskey="c">
                    <img src="images/new.png" alt="<%=umbraco.ui.Text("general", "create")%>" /><span><%=umbraco.ui.Text("general", "create")%>...</span>
                </button>
            </div>
            <asp:Panel ID="FindDocuments" runat="server">
                <div style="float: left; margin-left: 20px;">
                    <uc1:quickSearch ID="Search" runat="server"></uc1:quickSearch>
                </div>
            </asp:Panel>
            <div class="topBarButtons">
                <button onclick="UmbClientMgr.appActions().launchAbout();" class="topBarButton">
                    <img src="images/aboutNew.png" alt="about" /><span><%=umbraco.ui.Text("general", "about")%></span></button>
                <button onclick="UmbClientMgr.appActions().launchHelp('<%=this.getUser().Language%>', '<%=this.getUser().UserType.Name%>');"
                    class="topBarButton">
                    <img src="images/help.png" alt="Help" /><span><%=umbraco.ui.Text("general", "help")%></span></button>
                <button onclick="UmbClientMgr.appActions().logout();" class="topBarButton">
                    <img src="images/logout.png" alt="Log out" /><span><%=umbraco.ui.Text("general", "logout")%>:
                        <%=this.getUser().Name%></span></button>
            </div>
        </div>
    </div>
    <a id="treeToggle" href="#" onclick="toggleTree(this); return false;" title="Hide Treeview">
        &nbsp;</a>
    <div id="uiArea">
        <div id="leftDIV">
            <cc1:UmbracoPanel AutoResize="false" ID="treeWindow" runat="server" Height="380px"
                Width="200px" hasMenu="false">
                <umbraco:TreeControl runat="server" ID="JTree" ManualInitialization="true"></umbraco:TreeControl>
            </cc1:UmbracoPanel>
            <cc1:UmbracoPanel ID="PlaceHolderAppIcons" Text="Sektioner" runat="server" Height="140px"
                Width="200px" hasMenu="false" AutoResize="false">
                <ul id="tray">
                    <asp:Literal ID="plcIcons" runat="server"></asp:Literal>
                </ul>
            </cc1:UmbracoPanel>
        </div>
        <div id="rightDIV">
            <!-- umbraco dashboard -->
            <iframe name="right" id="right" marginwidth="0" marginheight="0" frameborder="0"
                scrolling="no" style="display: none; width: 100%; height: 100%;"></iframe>
        </div>
    </div>
    <script type="text/javascript">
    	  <asp:PlaceHolder ID="bubbleText" Runat="server"/>
    </script>
    <iframe src="keepalive.aspx" style="border: none; width: 1px; height: 1px; position: absolute;">
    </iframe>
    <div id="defaultSpeechbubble">
    </div>
    <div id="umbModalBox">
        <div id="umbModalBoxHeader">
        </div>
        <a href="#" id="umbracModalBoxClose" class="jqmClose">&times;</a>
        <div id="umbModalBoxContent">
            <iframe frameborder="0" id="umbModalBoxIframe" src=""></iframe>
        </div>
    </div>
    <script type="text/javascript">

        //used for deeplinking to specific content whilst still showing the tree
        var initApp = '<%=umbraco.presentation.UmbracoContext.Current.Request.QueryString["app"]%>';
        var rightAction = '<%=umbraco.presentation.UmbracoContext.Current.Request.QueryString["rightAction"]%>';
        var rightActionId = '<%=umbraco.presentation.UmbracoContext.Current.Request.QueryString["id"]%>';

        jQuery(document).ready(function () {

            UmbClientMgr.setUmbracoPath("<%=umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) %>");

            //call wresize first for IE/FF resize issues
            jQuery(window).wresize(function () { resizePage(); });
            resizePage();

            jQuery("#umbracoMainPageBody").css("background", "#fff");

            //wire up the history mgr
            UmbClientMgr.historyManager().addEventHandler("navigating", function (e, app) {
                //show modal wait dialog. TODO: Finish this
                //jQuery("<div id='appLoading'>&nbsp;</div>").appendTo("body")
                //    .ModalWindowShow("", false, 300, 100, 300, 0)
                //    .closest(".umbModalBox").css("border", "none");

                UmbClientMgr.appActions().shiftApp(app, uiKeys['sections_' + app]);
            });


            if (rightAction != '') {
                //if an action is specified, then load it
                UmbClientMgr.historyManager().addHistory(initApp != "" ? initApp :
                                                        UmbClientMgr.historyManager().getCurrent() != "" ? UmbClientMgr.historyManager().getCurrent() :
                                                        "<%=DefaultApp%>", true);
                UmbClientMgr.contentFrame(rightAction + ".aspx?id=" + rightActionId);
            }
            else {
                UmbClientMgr.historyManager().addHistory(initApp != "" ? initApp :
                                                        UmbClientMgr.historyManager().getCurrent() != "" ? UmbClientMgr.historyManager().getCurrent() :
                                                        "<%=DefaultApp%>", true);
            }



            jQuery("#right").show();
        });


        // Handles single vs double click on application item icon buttons...

        function appItemSingleClick(itemName) {
            UmbClientMgr.historyManager().addHistory(itemName);
            return false;
        }
        function appItemDoubleClick(itemName) {
            //When double clicking, we'll clear the tree cache so that it loads the dashboard
            UmbClientMgr.mainTree().clearTreeCache();
            UmbClientMgr.historyManager().addHistory(itemName);
            return false;
        }
        function appClick(appItem) {
            var that = this;
            setTimeout(function () {
                var dblclick = parseInt($(that).data('double'), 10);
                if (dblclick > 0) {
                    $(that).data('double', dblclick - 1);
                } else {
                    appItemSingleClick.call(that, appItem);
                }
            }, 300);
            return false;
        }
        function appDblClick(appItem) {
            $(this).data('double', 2);
            appItemDoubleClick.call(this, appItem);
            return false;
        }

    </script>
</body>
</html>
