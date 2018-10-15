<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" MasterPageFile="../../masterpages/umbracoPage.Master"
    Title="Package and export content" CodeBehind="editPackage.aspx.cs" Inherits="umbraco.presentation.developer.packages._Default" %>

<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        var updateMethod = "";
        var contentOrMediaId = "";
        var windowChooser;
        var treePickerId = -1;
        var prefix;

        function addfileJs() {
            if (document.getElementById("<%= packageFilePathNew.ClientID %>").value == '') {
                alert("Please pick a file by clicking the folder Icon, before clicking the 'add' button");
            }
        }
    </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc2:TabView ID="TabView1" runat="server" Width="552px" Height="392px"></cc2:TabView>
    <cc2:Pane ID="Pane1" runat="server">
        <cc2:PropertyPanel runat="server" ID="pp_name" Text="Package Name">
            <asp:TextBox ID="packageName" runat="server" Width="230px" CssClass="guiInputText"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator0" runat="server" EnableClientScript="false"
                ControlToValidate="packageName">*</asp:RequiredFieldValidator>
        </cc2:PropertyPanel>
        <cc2:PropertyPanel runat="server" ID="pp_url" Text="Package Url">
            <asp:TextBox ID="packageUrl" runat="server" Width="230px" CssClass="guiInputText"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" EnableClientScript="false"
                ControlToValidate="packageUrl">*</asp:RequiredFieldValidator>
        </cc2:PropertyPanel>
        <cc2:PropertyPanel runat="server" ID="pp_version" Text="Package Version">
            <asp:TextBox ID="packageVersion" runat="server" Width="230px" CssClass="guiInputText"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" EnableClientScript="false"
                ControlToValidate="packageVersion">*</asp:RequiredFieldValidator>
        </cc2:PropertyPanel>
        <cc2:PropertyPanel runat="server" ID="pp_icon" Text="Package Icon URL">
            <asp:TextBox ID="iconUrl" runat="server" Width="230px" CssClass="guiInputText"></asp:TextBox>            
        </cc2:PropertyPanel>
        <cc2:PropertyPanel runat="server" ID="pp_file" Text="Package file (.zip):">
            <asp:Literal ID="packageUmbFile" runat="server" />
        </cc2:PropertyPanel>
         
    </cc2:Pane>
    
    <cc2:Pane ID="Pane5" runat="server">
        <cc2:PropertyPanel runat="server" ID="pp_umbracoVersion" Text="Umbraco Target Version">
            <asp:TextBox ID="umbracoVersion" runat="server" Width="230px" CssClass="guiInputText"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" EnableClientScript="false"
                ControlToValidate="umbracoVersion">*</asp:RequiredFieldValidator>
            <asp:RegularExpressionValidator ID="VersionValidator" runat="server" EnableClientScript="false"
                ControlToValidate="umbracoVersion" ValidationExpression="^\d+\.\d+\.\d+$">Invalid version number (eg. 7.5.0)</asp:RegularExpressionValidator>
        </cc2:PropertyPanel>        
    </cc2:Pane>

    <cc2:Pane ID="Pane1_1" runat="server">
        <cc2:PropertyPanel runat="server" ID="pp_author" Text="Author Name" >
            <asp:TextBox ID="packageAuthorName" runat="server" Width="230px" CssClass="guiInputText"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" EnableClientScript="false"
                ControlToValidate="packageAuthorName">*</asp:RequiredFieldValidator>
        </cc2:PropertyPanel>
        <cc2:PropertyPanel runat="server" ID="pp_author_url" Text="Author url">
            <asp:TextBox ID="packageAuthorUrl" runat="server" Width="230px" CssClass="guiInputText"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" EnableClientScript="false"
                ControlToValidate="packageAuthorUrl">*</asp:RequiredFieldValidator>
        </cc2:PropertyPanel>
    </cc2:Pane>

    <cc2:Pane ID="Pane1_2" runat="server">
        <cc2:PropertyPanel runat="server" ID="pp_licens" Text="License Name:">
            <asp:TextBox ID="packageLicenseName" runat="server" Width="230px" CssClass="guiInputText"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" EnableClientScript="false"
                ControlToValidate="packageLicenseName">*</asp:RequiredFieldValidator>
        </cc2:PropertyPanel>
        <cc2:PropertyPanel runat="server" ID="pp_license_url" Text="License url:">
            <asp:TextBox ID="packageLicenseUrl" runat="server" Width="230px" CssClass="guiInputText"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" EnableClientScript="false"
                ControlToValidate="packageLicenseUrl">*</asp:RequiredFieldValidator>
        </cc2:PropertyPanel>
    </cc2:Pane>
    <cc2:Pane ID="Pane1_3" runat="server">
        <cc2:PropertyPanel runat="server" ID="pp_readme" Text="Readme">
            <asp:TextBox ID="packageReadme" TextMode="MultiLine" Rows="10" Width="460px" CssClass="guiInputText"
                runat="server"></asp:TextBox>
        </cc2:PropertyPanel>
    </cc2:Pane>
    <cc2:Pane ID="Pane2" runat="server">
        <cc2:PropertyPanel runat="server" ID="pp_content" Text="Content">
            <asp:PlaceHolder ID="content" runat="server"></asp:PlaceHolder>
            <br />
            <asp:CheckBox ID="packageContentSubdirs" runat="server" />
            <asp:Label ID="packageContentSubdirsLabel" Text="Include all child nodes" AssociatedControlID="packageContentSubdirs" runat="server" />
        </cc2:PropertyPanel>
    </cc2:Pane>
    <cc2:Pane ID="Pane2_1" runat="server">
        <cc2:PropertyPanel runat="server" Text="Document Types">
            <asp:CheckBoxList ID="documentTypes" runat="server" />
        </cc2:PropertyPanel>
    </cc2:Pane>
    <cc2:Pane ID="Pane2_2" runat="server">
        <cc2:PropertyPanel runat="server" Text="Templates">
            <asp:CheckBoxList ID="templates" runat="server" />
        </cc2:PropertyPanel>
    </cc2:Pane>
    <cc2:Pane ID="Pane2_3" runat="server">
        <cc2:PropertyPanel runat="server" Text="Stylesheets">
            <asp:CheckBoxList ID="stylesheets" runat="server" />
        </cc2:PropertyPanel>
    </cc2:Pane>
    <cc2:Pane ID="Pane2_4" runat="server">
        <cc2:PropertyPanel runat="server" Text="Macros">
            <asp:CheckBoxList ID="macros" runat="server" />
        </cc2:PropertyPanel>
    </cc2:Pane>
    <cc2:Pane ID="Pane2_5" runat="server">
        <cc2:PropertyPanel runat="server" Text="Languages">
            <asp:CheckBoxList ID="languages" runat="server" />
        </cc2:PropertyPanel>
    </cc2:Pane>
    <cc2:Pane ID="Pane2_6" runat="server">
        <cc2:PropertyPanel runat="server" Text="Dictionary Items">
            <asp:CheckBoxList ID="dictionary" runat="server" />
        </cc2:PropertyPanel>
    </cc2:Pane>
    <cc2:Pane ID="Pane2_7" runat="server">
        <cc2:PropertyPanel runat="server" Text="Data types">
            <asp:CheckBoxList ID="cbl_datatypes" runat="server" />
        </cc2:PropertyPanel>
    </cc2:Pane>
    <cc2:Pane ID="Pane3" runat="server">
        <table border="0" style="width: 100%;">
            <tr>
                <td>
                    <strong style="color: Red;">Remember:</strong> .xslt and .ascx files for your macros
                    will be added automaticly, but you will still need to add <strong>assemblies</strong>,
                    <strong>images</strong> and <strong>script files</strong> manually to the list below.
                </td>
            </tr>
        </table>
    </cc2:Pane>
    <cc2:Pane ID="Pane3_1" runat="server">
        <table border="0" style="width: 100%;">
            <tr>
                <td class="propertyHeader">
                    Absolute path to file (ie: /bin/umbraco.bin)
                </td>
                <td class="propertyHeader" />
            </tr>
            <asp:Repeater ID="packageFilesRepeater" runat="server">
                <ItemTemplate>
                    <tr>
                        <td class="propertyContent">
                            <asp:TextBox runat="server" ID="packageFilePath" Enabled="false" Width="330px" CssClass="guiInputText"
                                Text='<%#DataBinder.Eval(Container, "DataItem")%>' />
                        </td>
                        <td class="propertyContent">
                            <asp:Button OnClick="deleteFileFromPackage" ID="delete" Text="Delete" runat="server"
                                CssClass="btn btn-danger" />
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
            <tr>
                <td class="propertyContent">
                    <asp:TextBox runat="server" ID="packageFilePathNew" Width="330px" CssClass="guiInputText"
                        Text='' />
                    <a href="#" onclick="UmbClientMgr.openModalWindow('developer/packages/directoryBrowser.aspx?target=<%= packageFilePathNew.ClientID %>','Choose a file or a folder', true, 400, 500); return false;"
                        style="border: none;">
                        <i class="icon icon-folder"></i>
                        </a>
                </td>
                <td class="propertyContent">
                    <asp:Button ID="createNewFilePath" OnClientClick="addfileJs()" Text="Add" OnClick="addFileToPackage"
                        runat="server" CssClass="btn" />
                </td>
            </tr>
        </table>
    </cc2:Pane>
    <cc2:Pane ID="Pane3_2" runat="server">
        <table border="0" style="width: 100%;">
            <tr>
                <td class="propertyHeader" valign="top">
                    Load control after installation (ex: /usercontrols/installer.ascx)
                </td>
            </tr>
            <tr>
                <td class="propertyContent">
                    <asp:TextBox ID="packageControlPath" Width="330px" CssClass="guiInputText" runat="server" />
                    <a href="#" onclick="UmbClientMgr.openModalWindow('developer/packages/directoryBrowser.aspx?target=<%= packageControlPath.ClientID %>','Choose a file or a folder', true, 500, 400); return false;"
                        style="border: none;">
                         <i class="icon icon-folder"></i>
                      </a>
                </td>
            </tr>
        </table>
    </cc2:Pane>
    <cc2:Pane ID="Pane4" runat="server">
        <table border="0" style="width: 100%;">
            <tr>
                <td>
                    <p>
                        Here you can add custom installer / uninstaller events to perform certain tasks
                        during installation and uninstallation.
                        <br />
                        All actions are formed as a xml node, containing data for the action to be performed.
                        <a href="https://our.umbraco.com/documentation/Reference/Packaging/
" target="_blank">Package actions documentation</a>
                    </p>
                    <asp:CustomValidator ID="actionsVal" runat="server" OnServerValidate="validateActions"
                        ControlToValidate="tb_actions" ErrorMessage="Actions XML is malformed, either remove the text in the actions field or make sure it is correctly formed XML" />
                </td>
            </tr>
            <tr>
                <td class="propertyHeader">
                    Actions:
                </td>
            </tr>
            <tr>
                <td class="propertyContent">
                    <asp:TextBox ID="tb_actions" TextMode="MultiLine" Rows="14" Width="100%" CssClass="guiInputText"
                        runat="server"></asp:TextBox>
                </td>
            </tr>
        </table>
    </cc2:Pane>
    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>
</asp:Content>
