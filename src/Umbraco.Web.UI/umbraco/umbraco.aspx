<%@ Page Trace="false" Language="c#" CodeBehind="umbraco.aspx.cs" AutoEventWireup="True"
    Inherits="Umbraco.Web.UI.Umbraco.Umbraco" %>

<%@ Import Namespace="System.Web.Script.Serialization" %>

<%@ Register Src="controls/Tree/TreeControl.ascx" TagName="TreeControl" TagPrefix="umbraco" %>
<%@ Import Namespace="StackExchange.Profiling" %>
<%@ Import Namespace="Umbraco.Core.Profiling" %>
<%@ Import Namespace="Umbraco.Web" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="uc1" TagName="quickSearch" Src="Search/QuickSearch.ascx" %>
<%@ Register TagPrefix="cdf" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="umbClient" Namespace="Umbraco.Web.UI.Bundles" Assembly="umbraco" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Umbraco CMS -
        <%=Request.Url.Host.ToLower().Replace("www.", "") %></title>
    <meta content="IE=edge,chrome=1" http-equiv="X-UA-Compatible" />
    <link rel="icon" type="image/png" href="<%#umbraco.GlobalSettings.Path + "/Images/PinnedIcons/umb.ico" %>" />
    <cc1:UmbracoClientDependencyLoader runat="server" ID="ClientLoader" />
    
    <cdf:CssInclude ID="CssInclude1" runat="server" FilePath="css/umbracoGui.css" PathNameAlias="UmbracoRoot" />
    <cdf:CssInclude ID="CssInclude2" runat="server" FilePath="modal/style.css" PathNameAlias="UmbracoClient" />    
    
    <umbClient:JsApplicationLib runat="server"/>
    <umbClient:JsJQueryCore runat="server"/>
    <umbClient:JsUmbracoApplicationCore runat="server"/>
    <umbClient:JsJQueryPlugins runat="server"/>
    <umbClient:JsUmbracoApplicationUI runat="server"/>
    
    <cdf:JsInclude ID="JsInclude11" runat="server" FilePath="js/language.aspx" PathNameAlias="UmbracoRoot" />
    
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
        <div id="logout-warning" class="notice" style="display: none; text-align: center">
            <h3 style="margin-bottom: 3px;">
                <%= umbraco.ui.Text("lockout", "lockoutWillOccur")%>
                <span id="logout-warning-counter"></span><a href="#" onclick="umbracoRenewSession();">
                    <%= umbraco.ui.Text("lockout", "renewSession")%></a>.</h3>
        </div>
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
                <button onclick="UmbClientMgr.appActions().launchHelp('<%=UmbracoUser.Language%>', '<%=UmbracoUser.UserType.Name%>');"
                    class="topBarButton">
                    <img src="images/help.png" alt="Help" /><span><%=umbraco.ui.Text("general", "help")%></span></button>
                <form action="logout.aspx" method="get" style="display: inline;" >
                    <button class="topBarButton" type="submit">
                        <img src="images/logout.png" alt="Log out" />
                        <span><%=umbraco.ui.Text("general", "logout")%>:<%=UmbracoUser.Name%></span>
                    </button>
                    <input type="hidden" value="<%=Security.UmbracoUserContextId %>" name="t" id="t"/>
                </form>
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
    <div id="logout-refresh" style="display: none;">
        <p>
            <%= umbraco.ui.Text("general","locked").ToUpper() %></p>
        <div id="sessionrefreshpassword">
            <label for="sessionpass">
                <%= umbraco.ui.Text("general","password") %></label><input name="sessionpass" type="password" />
        </div>
        <div id="sessionrefreshbuttons">
            <button id="renew-session" onclick="javascript:umbracoSessionRenewCheckPassword();">
                <%= umbraco.ui.Text("general","renew") %></button>
            <%= umbraco.ui.Text("general","or") %>
            <a href="#" onclick="javascript:umbracoSessionLogout();">
                <%= umbraco.ui.Text("general","logout") %></a>
        </div>
    </div>
    <script type="text/javascript">

        //used for deeplinking to specific content whilst still showing the tree
        var initApp = '<%=InitApp%>';
        var rightAction = '<%=RightAction%>';
        var rightActionId = '<%=RightActionId%>';
        var base = '<%=string.Format("{0}/", Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco))%>';
        var url = '';
        if (rightActionId && rightActionId != '') {
            url = base + rightAction + ".aspx?id=" + rightActionId;
        } else {
            url = base + rightAction;
        }
        jQuery(document).ready(function () {

            UmbClientMgr.setUmbracoPath("<%=Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco) %>");

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

                UmbClientMgr.appActions().shiftApp(app, uiKeys['sections_' + app] || app);
            });


            if (rightAction != '') {
                //if an action is specified, then load it
                UmbClientMgr.historyManager().addHistory(initApp != "" ? initApp :
                                                        UmbClientMgr.historyManager().getCurrent() != "" ? UmbClientMgr.historyManager().getCurrent() :
                                                        "<%=DefaultApp%>", true);

                //use a small timeout to handle load delays
                //ref: http://our.umbraco.org/forum/developers/api-questions/32249-Direct-link-to-back-office-page
                var timer = setTimeout(function () {
                    UmbClientMgr.contentFrame(url);
                }, 200);
                
            }
            else {
                UmbClientMgr.historyManager().addHistory(initApp != "" ? initApp :
                                                        UmbClientMgr.historyManager().getCurrent() != "" ? UmbClientMgr.historyManager().getCurrent() :
                                                        "<%=DefaultApp%>", true);
            }



            jQuery("#right").show();


        });


        // *** NEW KEEP ALIVE - Should be moved to app manager *** */
        var failedAttempts = 0;
        var keepAliveInterval;
        beginKeepAlive();

        function beginKeepAlive() {
            keepAliveInterval = window.setInterval(keepAlive, 10000);
        }
        function pauseKeepAlive() {
            clearInterval(keepAliveInterval);
        }

        function keepAlive() {
            umbraco.presentation.webservices.legacyAjaxCalls.GetSecondsBeforeUserLogout(validateUserTimeout, keepAliveError);
        }

        function validateUserTimeout(secondsBeforeTimeout) {
            // at succesful attempts we'll always reset the failedAttempts variable
            failedAttempts = 0;
            var logoutElement = jQuery("#logout-warning");
            // when five minutes left, show warning
            if (secondsBeforeTimeout < 300) {
                if (secondsBeforeTimeout <= 0) {
                    umbracoShowSessionRenewModal();
                } else {

                    logoutElement.fadeIn();
                    jQuery("#logout-warning-counter").html(secondsBeforeTimeout + ' seconds...');

                    // when two mintutes left make warning RED
                    if (secondsBeforeTimeout <= 120) {
                        logoutElement.addClass('error');
                        logoutElement.removeClass('notice');
                    }
                }
            } else {
                logoutElement.fadeOut().removeClass('error').addClass('notice');
            }
        }

        // We add one failed attempt as tolerance level as a re-compilation could cause a timeout or an error with webservices not ready
        function keepAliveError(err) {
            if (failedAttempts == 0)
                failedAttempts++;
            else {
                umbracoShowSessionRenewModal();
            }
        }

        function umbracoRenewSession() {
            umbraco.presentation.webservices.legacyAjaxCalls.RenewUmbracoSession(
                function () {
                    jQuery("#logout-warning").fadeOut().removeClass('error').addClass('notice');
                },
                umbracoShowSessionRenewModal);

        }

        function umbracoShowSessionRenewModal() {

            pauseKeepAlive();
            jQuery("#logout-warning").fadeOut().removeClass('error').addClass('notice');

            jQuery("#sessionrefreshpassword input").attr("style", "");
            jQuery("#sessionrefreshpassword label").click(function () { jQuery(this).hide(); jQuery(this).next("input").focus(); });
            jQuery("#sessionrefreshpassword input").click(function () { jQuery(this).prev("label").hide(); });
            jQuery("#sessionrefreshpassword input").blur(function () { if (jQuery(this).val() == "") { jQuery(this).prev("label").show(); } });

            jQuery("#sessionrefreshpassword input").keypress(function (e) {
                if (jQuery("#sessionrefreshpassword label").is(":visible")) {
                    jQuery("#sessionrefreshpassword label").hide()
                }
                code = (e.keyCode ? e.keyCode : e.which);
                if (code == 13) {
                    jQuery("#renew-session").click();
                    e.preventDefault();
                    return false;
                }

            });

            jQuery("#logout-refresh").fullmodal();
        }

        function umbracoSessionRenewCheckPassword() {
            var password = jQuery("#sessionrefreshpassword input").val();

            if (password != "") {

                var data = {
                    username: <%= new JavaScriptSerializer().Serialize(UmbracoUser.LoginName) %>, 
                    password: password
                };

                jQuery.ajax({
                    type: "POST",
                    url: "<%=Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco) %>/webservices/legacyAjaxCalls.asmx/ValidateUser",
                    data: JSON.stringify(data),
                    processData: false, 
                    success: function (result) {

                        if (result.d == true) {
                            // reset seconds
                            umbraco.presentation.webservices.legacyAjaxCalls.RenewUmbracoSession();

                            jQuery("#sessionrefreshpassword input").val("");
                            jQuery.fullmodal.close();
                            beginKeepAlive();
                        }
                        else {
                            umbracoSessionRenewCheckPasswordFail();
                        }

                    }
                });
            }
            else {
                umbracoSessionRenewCheckPasswordFail();
            }
        }

        function umbracoSessionRenewCheckPasswordFail() {
            jQuery("#sessionrefreshpassword").effect("shake", { times: 5, distance: 5 }, 80);
            jQuery("#sessionrefreshpassword input").attr("style", "border: 2px solid red;");

        }
        function umbracoSessionLogout() {

            //alert('Session has expired on server - can\'t renew. Logging out!');
            top.document.location.href = 'logout.aspx?t=<%=Security.UmbracoUserContextId%>';
        }

        function blink($target) {
            // Set the color the field should blink in 
            var backgroundColor = '#FBC2C4';
            var existingBgColor;

            // Load the current background color 
            existingBgColor = $target.css('background-color');

            // Set the new background color 
            $target.css('background-color', backgroundColor);

            // Set it back to old color after 500 ms 
            setTimeout(function () { $target.css('background-color', existingBgColor); }, 500);
        }

        // *** END *** */

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
    <%if(string.IsNullOrEmpty(Request["umbDebug"]) == false && umbraco.GlobalSettings.DebugMode)
      {
          Response.Write(Html.RenderProfiler());
      }%>
</body>
</html>
