<%@ Page Language="C#" MasterPageFile="../../masterpages/umbracoPage.Master" AutoEventWireup="true" CodeBehind="installedPackage.aspx.cs" Inherits="umbraco.presentation.developer.packages.installedPackage" %>
<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function toggleDiv(id, gotoDiv) {
            var div = document.getElementById(id);

            if (div.style.display == "none")
                div.style.display = "block";

            else
                div.style.display = "none";
        }

        function openDemo(link, id) {
            UmbClientMgr.openModalWindow("http://packages.umbraco.org/viewPackageData.aspx?id=" + id, link.innerHTML, true, 750, 550)
        }

    </script>

    <style type="text/css">
        .propertyItemheader {
            width: 250px;
        }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">


    <cc2:Tabview ID="Panel1" Text="Installed package" runat="server" Width="496px" Height="584px">


       


            <cc2:Pane ID="pane_meta" runat="server" Text="Package meta data">

                <cc2:PropertyPanel ID="pp_name" runat="server">
                    <asp:Literal ID="lt_packagename" runat="server" />
                </cc2:PropertyPanel>
                <cc2:PropertyPanel ID="pp_version" runat="server">
                    <asp:Literal ID="lt_packageVersion" runat="server" />
                </cc2:PropertyPanel>
                <cc2:PropertyPanel ID="pp_author" runat="server">
                    <asp:Literal ID="lt_packageAuthor" runat="server" />
                </cc2:PropertyPanel>

                <cc2:PropertyPanel ID="pp_documentation" Visible="false" runat="server">
                    <asp:HyperLink id="hl_docLink" Target="_blank" runat="server" />
                    <asp:LinkButton id="lb_demoLink" OnClientClick="" runat="server" />
                </cc2:PropertyPanel>

                <cc2:PropertyPanel ID="pp_repository" Visible="false" runat="server">
                    <asp:HyperLink id="hl_packageRepo" runat="server" />
                </cc2:PropertyPanel>

                <cc2:PropertyPanel ID="pp_readme" runat="server">
                    <div style="position: relative; background: #fff; padding: 3px; border: 1px solid #ccc; width: 400px; white-space: normal !Important; overflow: auto;">
                        <asp:Literal ID="lt_readme" runat="server" /></div>
                </cc2:PropertyPanel>


            </cc2:Pane>

            <cc2:Pane ID="pane_versions" runat="server" Text="Package version history" Visible="false">
                <cc2:PropertyPanel ID="pp_versions" runat="server">
                    <asp:Repeater ID="rptr_versions" runat="server">
                        <headertemplate><ul></headertemplate>
                        <itemtemplate><li><a href="#"><%# ((umbraco.cms.businesslogic.packager.InstalledPackage)Container.DataItem).Data.Name %></a></li></itemtemplate>
                        <footertemplate></ul></footertemplate>
                    </asp:Repeater>
                </cc2:PropertyPanel>
            </cc2:Pane>
            
        <cc2:Pane ID="pane_upgrade" runat="server" Text="Upgrade package" Visible="false">
    
            <cc2:PropertyPanel runat="server">
              <p>
                  <%= umbraco.ui.Text("packager", "packageUpgradeText") %>
              </p>
            </cc2:PropertyPanel>
    
            <cc2:PropertyPanel ID="pp_upgradeInstruction" Text="Upgrade instructions" runat="server">
                  <p>
                    <asp:Literal ID="lt_upgradeReadme" runat="server" />
                  </p>
            
                  <p>
                    <asp:Button ID="bt_gotoUpgrade" Text="Download update from the repository" runat="server" UseSubmitBehavior="false" />
                  </p>
            </cc2:PropertyPanel>
        </cc2:Pane>

            <cc2:Pane ID="pane_noItems" Visible="false" runat="server" Text="Uninstaller doesn't contain any items">
                <div class="guiDialogNormal" style="margin: 10px">

                    <%= umbraco.ui.Text("packager", "packageNoItemsText") %>

                    <p>
                        <asp:Button ID="bt_deletePackage" OnClick="delPack" runat="server" Text="Remove uninstaller" />
                    </p>
                </div>

            </cc2:Pane>

            
            <cc2:Pane ID="pane_uninstall" runat="server" Text="Uninstall items installed by this package">
                <p>
                    <%= umbraco.ui.Text("packager", "packageUninstallText") %>
                </p>

                <cc2:PropertyPanel runat="server" Text="Document Types" ID="pp_docTypes">
                    <asp:CheckBoxList ID="documentTypes" runat="server" />
                </cc2:PropertyPanel>

                <cc2:PropertyPanel runat="server" Text="Templates" ID="pp_templates">
                    <asp:CheckBoxList ID="templates" runat="server" />
                </cc2:PropertyPanel>

                <cc2:PropertyPanel runat="server" Text="Stylesheets" ID="pp_css">
                    <asp:CheckBoxList ID="stylesheets" runat="server" />
                </cc2:PropertyPanel>

                <cc2:PropertyPanel runat="server" Text="Macros" ID="pp_macros">
                    <asp:CheckBoxList ID="macros" runat="server" />
                </cc2:PropertyPanel>

                <cc2:PropertyPanel ID="pp_files" runat="server" Text="Files">
                    <asp:CheckBoxList ID="files" runat="server" />
                </cc2:PropertyPanel>

                <cc2:PropertyPanel ID="pp_di" runat="server" Text="Dictionary Items">
                    <asp:CheckBoxList ID="dictionaryItems" runat="server" />
                </cc2:PropertyPanel>

                <cc2:PropertyPanel ID="pp_dt" runat="server" Text="Data types">
                    <asp:CheckBoxList ID="dataTypes" runat="server" />
                </cc2:PropertyPanel>

                <cc2:PropertyPanel ID="pp_confirm" runat="server" Text="&nbsp;">
                        <asp:Button ID="bt_confirmUninstall" OnClick="confirmUnInstall" OnClientClick="$('#loadingbar').show()" Text="Confirm uninstall" CssClass="btn btn-primary" runat="server" />
                        <div id="loadingbar" style="display: none">
                            <div class="umb-loader-wrapper">
                                <cc2:ProgressBar ID="progbar" runat="server" Title="Please wait..." />
                            </div>
                        </div>
                </cc2:PropertyPanel>
            </cc2:Pane>

            <cc2:Pane id="pane_uninstalled" runat="server" Visible="false">
                <div style="margin: 10px;">
                    <p><%= umbraco.ui.Text("packager", "packageUninstalledText") %></p>
                </div>
             </cc2:Pane>
    </cc2:Tabview>
</asp:Content>
