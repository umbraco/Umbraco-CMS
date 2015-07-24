<%@ Page Language="c#" MasterPageFile="../../masterpages/umbracoPage.Master"
    AutoEventWireup="True" Inherits="umbraco.presentation.developer.packages.Installer"
    Trace="false" ValidateRequest="false" %>
<%@ Import Namespace="umbraco" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">

    <script type="text/javascript">
        function enableButton() {

            var f = jQuery("#<%= file1.ClientID %>");
            var b = jQuery("#<%= ButtonLoadPackage.ClientID %>");
            var cb = jQuery("#cb");


            if (f.val() != "" && cb.attr("checked"))
                b.attr("disabled", false);
            else
                b.attr("disabled", true);
        }
        
        $(document).ready(function () {
            $('.toggle-report').click(function () {
                $(this).next().toggle();
            });
        });
    </script>

</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc1:UmbracoPanel ID="Panel1" Text="Install package" runat="server" Width="496px"
        Height="584px">
       

        <cc1:Feedback ID="fb" Style="margin-top: 7px;" runat="server" />
        <cc1:Pane ID="pane_upload" runat="server" Text="Install from local package file">

            <cc1:PropertyPanel runat="server" Text="">
                <div class="alert alert-warning">
                    <h4>
                        Only install packages from sources you know and trust!</h4>
                    <p>
                        When installing an Umbraco package you should use the same caution as when you install
                        an application on your computer.</p>
                    <p>
                        A malicious package could damage your Umbraco installation just like a malicious
                        application can damage your computer.
                    </p>
                    <p>
                        It is <strong>recommended</strong> to install from the official Umbraco package
                        repository or a custom repository whenever it's possible.
                    </p>
                    <p>
                        <input type="checkbox" id="cb" onchange="enableButton();" />
                        <label for="cb" style="font-weight: bold">
                            I understand the security risks associated with installing a local package</label>
                    </p>
                </div>
            </cc1:PropertyPanel>

            <cc1:PropertyPanel ID="PropertyPanel9" Text="Choose a file" runat="server">
                <p>
                    <input id="file1" type="file" class="btn" name="file1" onchange="enableButton();"
                        runat="server" />
                    <br />

                    <small>
                        <%= umbraco.ui.Text("packager", "chooseLocalPackageText") %>
                    </small>
                </p>
            </cc1:PropertyPanel>
            <cc1:PropertyPanel runat="server" Text="&nbsp;">
                <asp:Button ID="ButtonLoadPackage" runat="server" Enabled="false" Text="Load Package"
                    OnClick="uploadFile"></asp:Button>
                <div id="loadingbar" style="display: none;">
                    <div class="umb-loader-wrapper">
                        <cc1:ProgressBar ID="progbar1" runat="server" Title="Please wait..." />
                    </div>
                </div>
            </cc1:PropertyPanel>
        </cc1:Pane>
        <cc1:Pane ID="pane_authenticate" runat="server" Visible="false" Text="Repository authentication">
            <cc1:PropertyPanel runat="server">
                <div class="alert alert-warning">
                    <p>
                        This repository requires authentication before you can download any packages from
                        it.<br />
                        Please enter email and password to login.
                    </p>
                </div>
            </cc1:PropertyPanel>
            <cc1:PropertyPanel runat="server" Text="Email">
                <asp:TextBox ID="tb_email" runat="server" /></cc1:PropertyPanel>
            <cc1:PropertyPanel ID="PropertyPanel1" runat="server" Text="Password">
                <asp:TextBox ID="tb_password" TextMode="Password" runat="server" /></cc1:PropertyPanel>
            <cc1:PropertyPanel ID="PropertyPanel2" runat="server">
                <asp:Button ID="Button1" OnClick="fetchProtectedPackage" Text="Login" runat="server" /></cc1:PropertyPanel>
        </cc1:Pane>

        <asp:Panel ID="pane_acceptLicense" runat="server" Visible="false">
            
            <cc1:Pane ID="pane_acceptLicenseInner" runat="server">
                
                <div class="alert alert-warning">
                    <p>
                        <strong>Please note:</strong> Installing a package containing several items and
                        files can take some time. Do not refresh the page or navigate away before, the installer
                        notifies you once the install is completed.
                    </p>
                </div>

                <cc1:PropertyPanel ID="PropertyPanel3" runat="server" Text="Name">
                    <asp:Label ID="LabelName" runat="server" /></cc1:PropertyPanel>
                <cc1:PropertyPanel ID="PropertyPanel5" runat="server" Text="Author">
                    <asp:Label ID="LabelAuthor" runat="server" /></cc1:PropertyPanel>
                <cc1:PropertyPanel ID="PropertyPanel4" runat="server" Text="More info">
                    <asp:Label ID="LabelMore" runat="server" /></cc1:PropertyPanel>
                <cc1:PropertyPanel ID="PropertyPanel6" runat="server" Text="License">
                    <asp:Label ID="LabelLicense" runat="server" /></cc1:PropertyPanel>
                <cc1:PropertyPanel ID="PropertyPanel7" runat="server" Text="Accept license">
                    <asp:CheckBox Text="Accept license" runat="server" ID="acceptCheckbox" /></cc1:PropertyPanel>
                <cc1:PropertyPanel ID="PropertyPanel8" runat="server" Text="Read me">
                    <asp:Literal ID="readme" runat="server"></asp:Literal>
                </cc1:PropertyPanel>

                <cc1:PropertyPanel ID="pp_unsecureFiles" runat="server" Visible="false" Text="&nbsp;">
                    
                    <div class="alert alert-error" style="width: 370px;">
                        <h4>Binary files in the package!</h4>
                        
                        <a class="toggle-report" href="#">Read more...</a>
                        <div style="display:none;">
                            <p>
                                This package contains .NET code. This is <strong>not unusual</strong> as .NET code
                                is used for any advanced functionality on an Umbraco powered website.</p>
                            <p>
                                However, if you <strong>don't know the author</strong> of the package or are unsure why this package
                                contains these files, it is adviced <strong>not to continue the installation</strong>.
                            </p>
                            <p>
                                <strong>The Files in question:</strong><br />
                                <ul>
                                    <asp:Literal ID="lt_files" runat="server" />
                                </ul>
                            </p>
                        </div>
                    </div>

                </cc1:PropertyPanel>

                <cc1:PropertyPanel ID="LegacyPropertyEditorPanel" runat="server" Visible="false" Text="&nbsp;">
                    <div class="alert alert-error" style="width: 370px;">
                        <h4>
                            Legacy Property editors detected</h4>
                        <a class="toggle-report" href="#">Read more...</a>
                        <div style="display:none;">
                            <p>
                                This package contains legacy property editors which are not compatible with Umbraco 7</p>
                            <p>
                                This package may not function correctly if the package developer has not indicated that 
                                it is compatible with version 7. Any DataTypes this package creates that do not have
                                a Version 7 compatible property editor will be converted to use a Label/NoEdit property editor.
                            </p>                            
                        </div>
                    </div>
                </cc1:PropertyPanel>

                <cc1:PropertyPanel ID="BinaryFileErrorsPanel" runat="server" Visible="false" Text="&nbsp;">
                    <div class="alert alert-error" style="width: 370px;">
                        <h4>
                            Binary file errors detected</h4>                        
                        <a class="toggle-report" href="#">Read more...</a>
                        <div style="display:none;">
                            <p>
                                This package contains .NET binary files that might not be compatible with this version of Umbraco.
                                If you aren't sure what these errors mean or why they are listed please contact the package creator.
                            </p>                            
                            <p>
                                <strong>Error report</strong><br />
                                <ul>
                                    <asp:Literal ID="BinaryFileErrorReport" runat="server" />
                                </ul>
                            </p>
                        </div>
                    </div>
                </cc1:PropertyPanel>
                <cc1:PropertyPanel ID="pp_macroConflicts" runat="server" Visible="false" Text="&nbsp;">
                    <div class="alert alert-error" style="width: 370px;">
                        <h4>
                            Macro Conflicts in the package!</h4>
                        <a class="toggle-report" href="#">Read more...</a>
                        <div style="display:none">
                            <p>
                                This package contains one or more macros which have the same alias as an existing one on your site, based on the Macro Alias.
                                </p>
                            <p>
                                If you choose to continue your existing macros will be replaced with the ones from this package. If you do not want to overwrite your existing macros you will need to change their alias.
                            </p>
                            <p>
                                <strong>The Macros in question:</strong><br />
                                <ul>
                                    <asp:Literal ID="ltrMacroAlias" runat="server" />
                                </ul>
                            </p>
                        </div>
                    </div>
                </cc1:PropertyPanel>

                <cc1:PropertyPanel ID="pp_templateConflicts" runat="server" Visible="false" Text="&nbsp;">
                    <div class="alert alert-error" style="width: 370px;">
                        <h4>
                            Template Conflicts in the package!</h4>
                        <a class="toggle-report" href="#">Read more...</a>
                        <div style="display:none">
                            <p>
                                This package contains one or more templates which have the same alias as an existing one on your site, based on the Template Alias.
                                </p>
                            <p>
                                If you choose to continue your existing template will be replaced with the ones from this package. If you do not want to overwrite your existing templates you will need to change their alias.
                            </p>
                            <p>
                                <strong>The Templates in question:</strong><br />
                                <ul>
                                    <asp:Literal ID="ltrTemplateAlias" runat="server" />
                                </ul>
                            </p>
                        </div>
                    </div>
                </cc1:PropertyPanel>

                <cc1:PropertyPanel ID="pp_stylesheetConflicts" runat="server" Visible="false" Text="&nbsp;">
                    <div class="alert alert-error" style="width: 370px;">
                        <h4>
                            Stylesheet Conflicts in the package!</h4>
                        <a class="toggle-report" href="#">Read more...</a>
                        <div style="display:none">
                            <p>
                                This package contains one or more stylesheets which have the same alias as an existing one on your site, based on the Stylesheet Name.
                                </p>
                            <p>
                                If you choose to continue your existing stylesheets will be replaced with the ones from this package. If you do not want to overwrite your existing stylesheets you will need to change their name.
                            </p>
                            <p>
                                <strong>The Stylesheets in question:</strong><br />
                                <ul>
                                    <asp:Literal ID="ltrStylesheetNames" runat="server" />
                                </ul>
                            </p>
                        </div>
                    </div>
                </cc1:PropertyPanel>

                <cc1:PropertyPanel runat="server" Text=" ">
                    <br />
                    <div id="installingMessage" style="display: none;">
                        <div class="umb-loader-wrapper">
                            <cc1:ProgressBar runat="server" ID="_progbar1" />
                        </div>
                        <br />
                        <em>Installing package, please wait...</em><br /><br />
                    </div>
                    <asp:Button ID="ButtonInstall" runat="server" Text="Install Package" CssClass="btn btn-primary" Enabled="False"
                        OnClick="startInstall"></asp:Button>
                </cc1:PropertyPanel>
            </cc1:Pane>

        </asp:Panel>
        <cc1:Pane ID="pane_installing" runat="server" Visible="false" Text="Installing package">
            <cc1:PropertyPanel runat="server">
                <cc1:ProgressBar runat="server" ID="progBar2" />
                <asp:Literal ID="lit_installStatus" runat="server" />
            </cc1:PropertyPanel>
        </cc1:Pane>

        <cc1:Pane ID="pane_optional" runat="server" Visible="false" />
        
        <cc1:Pane ID="pane_success" runat="server" Text="Package is installed" Visible="false">
            <cc1:PropertyPanel runat="server">
              
                <p>
                    All items in the package have been installed</p>
                <p>
                    Overview of what was installed can be found under "installed package" in the developer
                    section.</p>
                <p>
                    Uninstall is available at the same location.</p>
                <p>
                    <asp:Button Text="View installed package" ID="bt_viewInstalledPackage" runat="server" />
                    <asp:Literal ID="lit_authorUrl" runat="server" />
                </p>               

            </cc1:PropertyPanel>
        </cc1:Pane>

        <cc1:Pane ID="pane_refresh" runat="server" Text="Browser is reloading" Visible="false">
            <cc1:PropertyPanel runat="server">
                
                <div class="alert alert-block">
                    Please wait while the browser is reloaded...
                </div>

                <script type="text/javascript">
                    
                    //This is all a bit zany with double encoding because we have a URL in a hash (#) url part
                    // but it works and maintains query strings

                    var refreshQuery = decodeURIComponent("<%=RefreshQueryString%>");
                    var umbPath = "<%=GlobalSettings.Path%>";
                    setTimeout(function () {

                       

                        var mainWindow = UmbClientMgr.mainWindow();

                        //kill the tree and template cache
                        if (mainWindow.UmbClientMgr) {
                            mainWindow.UmbClientMgr._packageInstalled();
                        }

                        var baseUrl = mainWindow.location.href.substr(0, mainWindow.location.href.indexOf("#/developer/framed/"));
                        var framedUrl = baseUrl + "#/developer/framed/";
                        var refreshUrl = framedUrl + encodeURIComponent(encodeURIComponent(umbPath + "/developer/packages/installer.aspx?" + refreshQuery));

                        var redirectUrl = umbPath + "/ClientRedirect.aspx?redirectUrl=" + refreshUrl;

                        mainWindow.location.href = redirectUrl;

                    }, 2000);
                </script>

            </cc1:PropertyPanel>
        </cc1:Pane>
        <input id="tempFile" type="hidden" name="tempFile" runat="server" /><input id="processState"
            type="hidden" name="processState" runat="server" />
    </cc1:UmbracoPanel>
</asp:Content>
