<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="True" Inherits="umbraco.presentation.umbraco.dialogs.protectPage" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function updateLoginId() {
            var treePicker = window.showModalDialog('<%=umbraco.cms.presentation.Trees.TreeService.GetPickerUrl(true,"content","content")%>', 'treePicker', 'dialogWidth=350px;dialogHeight=300px;scrollbars=no;center=yes;border=thin;help=no;status=no')
	    if (treePicker != undefined) {
	        document.getElementById("loginId").value = treePicker;
	        if (treePicker > 0) {
	            umbraco.presentation.webservices.CMSNode.GetNodeName('<%=umbraco.BasePages.BasePage.umbracoUserContextID%>', treePicker, updateLoginTitle);
					} else
					    document.getElementById("loginTitle").innerHTML =  "<strong><%=umbraco.ui.Text("content", base.getUser())%></strong>";
                }
            }

            function updateLoginTitle(result) {
                document.getElementById("loginTitle").innerHTML = "<strong>" + result + "</strong> &nbsp;";
            }

            function updateErrorId() {
                var treePicker = window.showModalDialog('<%=umbraco.cms.presentation.Trees.TreeService.GetPickerUrl(true,"content","content")%>', 'treePicker', 'dialogWidth=350px;dialogHeight=300px;scrollbars=no;center=yes;border=thin;help=no;status=no')
			    if (treePicker != undefined) {
			        document.getElementById("errorId").value = treePicker;
			        if (treePicker > 0) {
			            umbraco.presentation.webservices.CMSNode.GetNodeName('<%=umbraco.BasePages.BasePage.umbracoUserContextID%>', treePicker, updateErrorTitle);
					} else
					    document.getElementById("errorTitle").innerHTML =  "<strong><%=umbraco.ui.Text("content", base.getUser())%></strong>";
                }
            }
            function updateErrorTitle(result) {
                document.getElementById("errorTitle").innerHTML = "<strong>" + result + "</strong> &nbsp;";
            }


            function toggleSimple() {
                if (document.getElementById("advanced").style.display != "none") {
                    document.getElementById("advanced").style.display = "none";
                    document.getElementById("simple").style.display = "none";
                    document.getElementById("simpleForm").style.display = "block";
                    document.getElementById("buttonSimple").style.display = "block";
                    togglePages();
                } else {
                    document.getElementById("advanced").style.display = "block";
                    document.getElementById("advanced").style.display = "block";
                    document.getElementById("simpleForm").style.display = "none";
                    document.getElementById("buttonSimple").style.display = "none";
                    document.getElementById("pagesForm").style.display = "none";
                }
            }

            function togglePages() {
                document.getElementById("pagesForm").style.display = "block";
            }

            function toggleAdvanced() {
                if (document.getElementById("simple").style.display != "none") {
                    document.getElementById("advanced").style.display = "none";
                    document.getElementById("simple").style.display = "none";
                    document.getElementById("advancedForm").style.display = "block";
                    document.getElementById("buttonAdvanced").style.display = "block";
                    togglePages();
                } else {
                    document.getElementById("simple").style.display = "block";
                    document.getElementById("advanced").style.display = "block";
                    document.getElementById("advancedForm").style.display = "none";
                    document.getElementById("pagesForm").style.display = "none";
                    document.getElementById("buttonAdvanced").style.display = "none";
                }
            }
    </script>

</asp:Content>


<asp:Content ContentPlaceHolderID="body" runat="server">
    <style> .umb-dialog { overflow: auto; } .umb-dialog-footer { position: relative; }</style>

    <input id="tempFile" type="hidden" name="tempFile" runat="server" />

    <cc1:Feedback ID="feedback" runat="server" />

    <asp:Panel ID="p_mode" runat="server" CssClass="pa-umb-overlay">

        <div class="umg-dialog-body">

            <cc1:Pane ID="pane_chooseMode" runat="server" Text="Choose how to restict access to this page">

            <div class="pa-select-type">
                <asp:RadioButton GroupName="mode" ID="rb_simple" runat="server" Checked="true" />

                <label for="body_rb_simple">
                    <h4 class="pa-access-header"><%= umbraco.ui.Text("publicAccess", "paSimple", base.getUser())%></h4>
                    <p class="pa-access-description"><%= umbraco.ui.Text("publicAccess", "paSimpleHelp", base.getUser())%></p>
                </label>
            </div>


            <div class="pa-select-type">
                <asp:RadioButton GroupName="mode" ID="rb_advanced" runat="server"/>

                <label for="body_rb_advanced">
                    <h4 class="pa-access-header"><%= umbraco.ui.Text("publicAccess", "paAdvanced", base.getUser())%></h4>
                    <p class="pa-access-description"><%= umbraco.ui.Text("publicAccess", "paAdvancedHelp", base.getUser())%></p>
                    <br />

                    <asp:Panel runat="server" Visible="false" ID="p_noGroupsFound" CssClass="alert alert-danger">
                        <span><%= umbraco.ui.Text("publicAccess", "paAdvancedNoGroups", UmbracoUser)%></span>
                    </asp:Panel>
                </label>
            </div>

            </cc1:Pane>
        </div>

        <div class="umb-dialog-footer btn-toolbar umb-btn-toolbar">
            <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
            <asp:Button ID="bt_selectMode" runat="server" Text="select" CssClass="btn btn-primary" OnClick="selectMode" />
        </div>
    </asp:Panel>


    <cc1:Pane ID="pane_simple" runat="server" Visible="false" Text="Set the login and password for this page" CssClass="pa-umb-overlay">

        <div class="pa-form">
            <cc1:PropertyPanel Text="Login" ID="pp_login" runat="server">
                <asp:TextBox ID="simpleLogin" runat="server" Width="250px"></asp:TextBox>
                <asp:Label runat="server" ID="SimpleLoginLabel" Visible="False"></asp:Label>
            </cc1:PropertyPanel>
        </div>

        <div class="pa-form">
            <cc1:PropertyPanel Text="Password" ID="pp_pass" runat="server">
                <asp:TextBox ID="simplePassword" runat="server" Width="250px"></asp:TextBox>
            </cc1:PropertyPanel>
        </div>

        <asp:CustomValidator CssClass="alert alert-danger" runat="server" ID="SimpleLoginNameValidator" Display="Dynamic" EnableViewState="False">
           <p class="alert">Member name already exists, click <asp:LinkButton runat="server" OnClick="ChangeOnClick" CssClass="btn btn-mini btn-warning">Change</asp:LinkButton> to use a different name or Update to continue</p>
        </asp:CustomValidator>
    </cc1:Pane>

    <cc1:Pane ID="pane_advanced" runat="server" Visible="false" Text="Role based protection">
        <cc1:PropertyPanel ID="PropertyPanel3" runat="server">
            <p><%= umbraco.ui.Text("publicAccess", "paSelectRoles", UmbracoUser)%></p>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel ID="PropertyPanel2" runat="server">
            <asp:PlaceHolder ID="groupsSelector" runat="server"></asp:PlaceHolder>
        </cc1:PropertyPanel>
    </cc1:Pane>

    <asp:Panel ID="p_buttons" runat="server" Visible="false" CssClass="pa-umb-overlay">
        <cc1:Pane runat="server" ID="pane_pages" Text="Select the pages that contain login form and error messages">

            <cc1:PropertyPanel runat="server" ID="pp_loginPage" CssClass="pa-select-pages">
                <small class="umb-detail">
                    <%=umbraco.ui.Text("paLoginPageHelp")%>
                </small>
                <div class="pa-choose-page">
                    <asp:PlaceHolder ID="ph_loginpage" runat="server" />
                </div>

                <asp:CustomValidator ErrorMessage="Please pick a login page" runat="server" ID="cv_loginPage" ForeColor="Red" />
            </cc1:PropertyPanel>


            <cc1:PropertyPanel runat="server" ID="pp_errorPage" CssClass="pa-select-pages">
                <small class="umb-detail">
                    <%=umbraco.ui.Text("paErrorPageHelp")%>
                </small>
                <div class="pa-choose-page">
                    <asp:PlaceHolder ID="ph_errorpage" runat="server" />
                </div>
                <asp:CustomValidator ErrorMessage="Please pick an error page" runat="server" ID="cv_errorPage"  ForeColor="Red" />
            </cc1:PropertyPanel>

        </cc1:Pane>


        <div class="umb-dialog-footer btn-toolbar umb-btn-toolbar">
            <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
            <asp:Button ID="bt_protect" CssClass="btn btn-primary" runat="server" OnCommand="protect_Click"></asp:Button>
            <asp:Button ID="bt_buttonRemoveProtection" CssClass="btn btn-danger" runat="server" Visible="False" OnClick="buttonRemoveProtection_Click" />
        </div>

    </asp:Panel>

    <input id="errorId" type="hidden" runat="server" /><input id="loginId" type="hidden" runat="server" />
</asp:Content>


<asp:Content ContentPlaceHolderID="footer" runat="server">
    <asp:PlaceHolder ID="js" runat="server"></asp:PlaceHolder>

    <script type="text/javascript">
        <asp:Literal Runat="server" ID="jsShowWindow"></asp:Literal>
    </script>
</asp:Content>
